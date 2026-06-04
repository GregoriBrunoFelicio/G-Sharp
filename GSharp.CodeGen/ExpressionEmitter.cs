using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;
using GSharp.Lexer;
using GSharp.TypeChecker;

namespace GSharp.CodeGen;

// Emits IL for all expression types in G#.
//
// ══════════════════════════════════════════════════════════════
//  STACK CONTRACT
// ══════════════════════════════════════════════════════════════
//
// EmitToStack (public) — always leaves exactly ONE boxed object on the stack.
//   Used by all callers that need a uniform object reference (PrintEmitter,
//   IfEmitter, function return values, array element stores, etc.).
//
// Emit (internal) — leaves the native CLR type on the stack and returns it.
//   Value types (int, float, double, bool, decimal) are left unboxed.
//   Reference types (string, object[], object) are left as-is.
//   Used by LetEmitter to store typed locals and by binary arithmetic
//   to chain native IL opcodes without boxing intermediate values.
//
// EmitToStack is a thin wrapper over Emit that boxes value types.
//
// ══════════════════════════════════════════════════════════════
public static class ExpressionEmitter
{
    // Public contract: always leaves exactly one boxed object on the stack.
    public static void EmitToStack(ILGenerator il, Expression expression, EmitContext ctx)
    {
        var clrType = Emit(il, expression, ctx);
        if (clrType.IsValueType)
            il.Emit(OpCodes.Box, clrType);
    }

    // Internal contract: emits the expression and returns the actual CLR type on the stack.
    // Value types are left unboxed; reference types are left as references.
    internal static Type Emit(ILGenerator il, Expression expression, EmitContext ctx)
    {
        switch (expression)
        {
            case LiteralExpression lit:
                return EmitLiteral(il, lit.Value);

            case BindingExpression v:
                return EmitBinding(il, v, ctx);

            case CallExpression call:
                EmitCall(il, call, ctx);
                return typeof(object);

            case QualifiedCallExpression qCall:
                EmitQualifiedCall(il, qCall, ctx);
                return typeof(object);

            case BinaryExpression b:
                return EmitBinary(il, b, ctx);

            case LetExpression letExpr:
                LetEmitter.Emit(il, letExpr, ctx);
                return typeof(object);

            case PrintExpression printExpr:
                PrintEmitter.Emit(il, printExpr, ctx);
                return typeof(object);

            case ForExpression forExpr:
                ForEmitter.Emit(il, forExpr, ctx);
                return typeof(object[]);

            case WhileExpression whileExpr:
                WhileEmitter.Emit(il, whileExpr, ctx);
                return typeof(object);

            case IfExpression ifExpr:
                IfEmitter.EmitToStack(il, ifExpr, ctx);
                return typeof(object);

            case FunctionDeclaration:
                il.Emit(OpCodes.Ldnull);
                return typeof(object);

            default:
                throw new NotSupportedException(
                    $"internal error: no emitter for expression '{expression.GetType().Name}'");
        }
    }

    // -------------------------------------------------------------------------
    // Literal emission — returns native CLR type, no boxing
    // -------------------------------------------------------------------------

    private static Type EmitLiteral(ILGenerator il, object value)
    {
        switch (value)
        {
            case int i:
                il.Emit(OpCodes.Ldc_I4, i);
                return typeof(int);

            case double d:
                il.Emit(OpCodes.Ldc_R8, d);
                return typeof(double);

            case bool b:
                il.Emit(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                return typeof(bool);

            case string s:
                il.Emit(OpCodes.Ldstr, s);
                return typeof(string);

            case float f:
                il.Emit(OpCodes.Ldc_R4, f);
                return typeof(float);

            case decimal m:
                EmitDecimalLiteral(il, m);
                return typeof(decimal);

            case object[] arr:
                ArrayEmitter.EmitToStack(il, arr);
                return typeof(object[]);

            default:
                throw new NotSupportedException(
                    $"internal error: no emitter for literal type '{value?.GetType().Name ?? "null"}'");
        }
    }

    // Decimal has no IL literal opcode — decomposes into bit fields and calls the constructor.
    //
    // decimal.GetBits() returns four int32 words:
    //   [0] = lo bits, [1] = mid bits, [2] = hi bits
    //   [3] = sign (bit 31) + scale (bits 16–23)
    private static void EmitDecimalLiteral(ILGenerator il, decimal value)
    {
        var bits  = decimal.GetBits(value);
        var lo    = bits[0];
        var mid   = bits[1];
        var hi    = bits[2];
        var sign  = (bits[3] & 0x80000000) != 0;
        var scale = (byte)((bits[3] >> 16) & 0x7F);

        var ctor = typeof(decimal).GetConstructor([
            typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte)
        ]) ?? throw new Exception("Decimal constructor not found");

        il.Emit(OpCodes.Ldc_I4, lo);
        il.Emit(OpCodes.Ldc_I4, mid);
        il.Emit(OpCodes.Ldc_I4, hi);
        il.Emit(sign ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ldc_I4_S, scale);
        il.Emit(OpCodes.Newobj, ctor);
    }

    // -------------------------------------------------------------------------
    // Binding emission — loads local or parameter, returns its actual CLR type
    // -------------------------------------------------------------------------

    private static Type EmitBinding(ILGenerator il, BindingExpression v, EmitContext ctx)
    {
        if (ctx.Parameters.TryGetValue(v.Name, out var paramIndex))
        {
            il.Emit(OpCodes.Ldarg, paramIndex);
            return typeof(object); // function parameters are always object
        }

        if (ctx.Locals.TryGetValue(v.Name, out var local))
        {
            il.Emit(OpCodes.Ldloc, local);
            return local.LocalType; // typed locals return their actual type (may be int, double, etc.)
        }

        if (ctx.FunctionAdapters.TryGetValue(v.Name, out var adapter))
        {
            EmitFunctionValue(il, adapter);
            return typeof(object); // GSharpFunction is a reference type
        }

        throw new Exception($"Undefined binding: '{v.Name}'");
    }

    // -------------------------------------------------------------------------
    // Binary emission — uses direct IL opcodes when types are known numeric
    // -------------------------------------------------------------------------

    private static Type EmitBinary(ILGenerator il, BinaryExpression b, EmitContext ctx)
    {
        if (b.Operator is TokenType.And or TokenType.Or)
        {
            // AND / OR require short-circuit evaluation.
            // EmitLogicalShortCircuit stores the result in a bool local.
            var result = EmitLogicalShortCircuit(il, b, ctx);
            il.Emit(OpCodes.Ldloc, result);
            return typeof(bool); // unboxed bool; EmitToStack will box if needed
        }

        // When the TypeMap tells us both operands have the same concrete numeric type
        // AND we can verify the values will actually be unboxed at IL level,
        // emit direct opcodes (Add/Sub/Mul/Div) — no RuntimeHelpers, no boxing overhead.
        var nativeType = TryGetNativeArithmeticType(b, ctx);
        if (nativeType is not null)
        {
            Emit(il, b.Left,  ctx); // leaves native value on stack
            Emit(il, b.Right, ctx); // leaves native value on stack

            var opcode = b.Operator switch
            {
                TokenType.Plus     => OpCodes.Add,
                TokenType.Minus    => OpCodes.Sub,
                TokenType.Multiply => OpCodes.Mul,
                TokenType.Divide   => OpCodes.Div,
                _ => throw new InvalidOperationException(
                    $"internal error: '{b.Operator}' is not a native arithmetic operator")
            };

            il.Emit(opcode);
            return nativeType;
        }

        // Fallback: box both operands and dispatch through RuntimeHelpers.
        // RuntimeHelpers handles all type combinations at runtime (int+double, etc.).
        EmitToStack(il, b.Left,  ctx); // boxed object
        EmitToStack(il, b.Right, ctx); // boxed object

        var method = b.Operator switch
        {
            TokenType.GreaterThan        => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.GreaterThan)),
            TokenType.LessThan           => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.LessThan)),
            TokenType.GreaterThanOrEqual => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.GreaterThanOrEqual)),
            TokenType.LessThanOrEqual    => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.LessThanOrEqual)),
            TokenType.EqualEqual         => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.EqualEqual)),
            TokenType.NotEqual           => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.NotEqual)),
            TokenType.Plus               => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Add)),
            TokenType.Minus              => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Subtract)),
            TokenType.Multiply           => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Multiply)),
            TokenType.Divide             => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Divide)),
            _ => throw new NotSupportedException(
                $"internal error: no emitter for binary operator '{b.Operator}'")
        };

        il.Emit(OpCodes.Call, method!);
        return typeof(object); // RuntimeHelpers always returns a boxed object
    }

    // Returns the shared CLR type for native arithmetic if possible, null otherwise.
    //
    // Native arithmetic requires:
    //   1. The operator is +, -, *, / (comparison stays with RuntimeHelpers for now).
    //   2. Both operands have the same concrete GsType in the TypeMap (IntType, FloatType, DoubleType).
    //   3. Both operands will actually emit unboxed native values (not boxed object locals/params).
    //
    // Condition 3 prevents emitting Add on a boxed object from a loop variable or function
    // parameter, which would corrupt the IL stack.
    private static Type? TryGetNativeArithmeticType(BinaryExpression b, EmitContext ctx)
    {
        if (b.Operator is not (TokenType.Plus or TokenType.Minus or TokenType.Multiply or TokenType.Divide))
            return null;

        if (!ctx.TypeMap.TryGetValue(b.Left,  out var leftGsType) ||
            !ctx.TypeMap.TryGetValue(b.Right, out var rightGsType))
            return null;

        if (!WillEmitNativeValue(b.Left, ctx) || !WillEmitNativeValue(b.Right, ctx))
            return null;

        return (leftGsType, rightGsType) switch
        {
            (IntType,    IntType)    => typeof(int),
            (FloatType,  FloatType)  => typeof(float),
            (DoubleType, DoubleType) => typeof(double),
            _ => null
        };
    }

    // Returns true when Emit(expr) would leave an unboxed native value (not a boxed object).
    // Used to verify that native arithmetic opcodes are safe to use.
    private static bool WillEmitNativeValue(Expression expr, EmitContext ctx) => expr switch
    {
        LiteralExpression lit  => lit.Value is int or float or double or bool or decimal,
        BindingExpression v    => ctx.Locals.TryGetValue(v.Name, out var local)
                                  && local.LocalType.IsValueType,
        BinaryExpression bin   => TryGetNativeArithmeticType(bin, ctx) is not null,
        _ => false
    };

    // -------------------------------------------------------------------------
    // Call emission
    // -------------------------------------------------------------------------

    private static void EmitCall(ILGenerator il, CallExpression call, EmitContext ctx)
    {
        if (ctx.PrecompiledFunctions.TryGetValue(call.Callee, out var precompiled))
        {
            foreach (var arg in call.Arguments)
                EmitToStack(il, arg, ctx);
            il.Emit(OpCodes.Call, precompiled);
        }
        else if (ctx.Functions.ContainsKey(call.Callee))
        {
            foreach (var arg in call.Arguments)
                EmitToStack(il, arg, ctx);
            il.Emit(OpCodes.Call, ctx.Functions[call.Callee]);
        }
        else
        {
            EmitDelegateCall(il, call, ctx);
        }
    }

    private static void EmitQualifiedCall(ILGenerator il, QualifiedCallExpression qCall, EmitContext ctx)
    {
        var key = $"{qCall.Module}.{qCall.Function}";
        if (ctx.PrecompiledFunctions.TryGetValue(key, out var precompiledQ))
        {
            foreach (var arg in qCall.Arguments)
                EmitToStack(il, arg, ctx);
            il.Emit(OpCodes.Call, precompiledQ);
        }
        else if (ctx.Functions.TryGetValue(key, out var moduleMethod))
        {
            foreach (var arg in qCall.Arguments)
                EmitToStack(il, arg, ctx);
            il.Emit(OpCodes.Call, moduleMethod);
        }
        else
            throw new Exception($"Undefined function: '{key}'");
    }

    // -------------------------------------------------------------------------
    // Short-circuit AND / OR
    // -------------------------------------------------------------------------

    // Short-circuit means: if the result can be determined from the left side alone,
    // the right side is NOT evaluated.
    //
    //   AND: if left is false → result is false (skip right)
    //   OR:  if left is true  → result is true  (skip right)
    //
    // Returns a LocalBuilder(bool) holding the result.
    // The caller is responsible for loading it (and boxing if needed).
    private static LocalBuilder EmitLogicalShortCircuit(
        ILGenerator il, BinaryExpression expr, EmitContext ctx)
    {
        var result = il.DeclareLocal(typeof(bool));

        var shortCircuit = il.DefineLabel();
        var end          = il.DefineLabel();

        EmitToStack(il, expr.Left, ctx);
        il.Emit(OpCodes.Unbox_Any, typeof(bool));
        il.Emit(expr.Operator == TokenType.And ? OpCodes.Brfalse : OpCodes.Brtrue, shortCircuit);

        EmitToStack(il, expr.Right, ctx);
        il.Emit(OpCodes.Unbox_Any, typeof(bool));
        il.Emit(OpCodes.Stloc, result);
        il.Emit(OpCodes.Br, end);

        il.MarkLabel(shortCircuit);
        il.Emit(expr.Operator == TokenType.And ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Stloc, result);

        il.MarkLabel(end);
        return result;
    }

    // -------------------------------------------------------------------------
    // Higher-order function helpers
    // -------------------------------------------------------------------------

    // Wraps a static adapter method in a GSharpFunction so the function can be
    // passed as a first-class value.
    private static void EmitFunctionValue(ILGenerator il, MethodBuilder adapter)
    {
        var funcType   = typeof(Func<object[], object>);
        var funcCtor   = funcType.GetConstructors()[0];
        var gsFuncCtor = typeof(GSharpFunction).GetConstructor([funcType])!;

        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Ldftn, adapter);
        il.Emit(OpCodes.Newobj, funcCtor);
        il.Emit(OpCodes.Newobj, gsFuncCtor);
    }

    // Emits a call through a GSharpFunction value held in a local or parameter.
    // Arguments are packed into object[] and GSharpFunction.Call(object[]) is invoked.
    private static void EmitDelegateCall(ILGenerator il, CallExpression call, EmitContext ctx)
    {
        if (ctx.Parameters.TryGetValue(call.Callee, out var paramIdx))
            il.Emit(OpCodes.Ldarg, paramIdx);
        else if (ctx.Locals.TryGetValue(call.Callee, out var local))
            il.Emit(OpCodes.Ldloc, local);
        else
            throw new Exception($"Undefined function or binding: '{call.Callee}'");

        il.Emit(OpCodes.Castclass, typeof(GSharpFunction));

        il.Emit(OpCodes.Ldc_I4, call.Arguments.Count);
        il.Emit(OpCodes.Newarr, typeof(object));

        for (var i = 0; i < call.Arguments.Count; i++)
        {
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4, i);
            EmitToStack(il, call.Arguments[i], ctx);
            il.Emit(OpCodes.Stelem_Ref);
        }

        il.Emit(OpCodes.Callvirt, typeof(GSharpFunction).GetMethod(nameof(GSharpFunction.Call))!);
    }
}
