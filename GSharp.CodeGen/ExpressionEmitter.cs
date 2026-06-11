using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;
using GSharp.Lexer;
using GSharp.TypeChecker;

namespace GSharp.CodeGen;

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
    // Binding emission
    // -------------------------------------------------------------------------

    private static Type EmitBinding(ILGenerator il, BindingExpression v, EmitContext ctx)
    {
        if (ctx.Parameters.TryGetValue(v.Name, out var paramIndex))
        {
            il.Emit(OpCodes.Ldarg, paramIndex);
            return typeof(object);
        }

        if (ctx.Locals.TryGetValue(v.Name, out var local))
        {
            il.Emit(OpCodes.Ldloc, local);
            return local.LocalType;
        }

        if (ctx.FunctionAdapters.TryGetValue(v.Name, out var adapter))
        {
            EmitFunctionValue(il, adapter);
            return typeof(object);
        }

        throw new Exception($"Undefined binding: '{v.Name}'");
    }

    // -------------------------------------------------------------------------
    // Binary emission
    // -------------------------------------------------------------------------

    private static Type EmitBinary(ILGenerator il, BinaryExpression b, EmitContext ctx)
    {
        if (b.Operator is TokenType.And or TokenType.Or)
        {
            var result = EmitLogicalShortCircuit(il, b, ctx);
            il.Emit(OpCodes.Ldloc, result);
            return typeof(bool);
        }

        var nativeType = TryGetNativeArithmeticType(b, ctx);
        if (nativeType is not null)
        {
            Emit(il, b.Left,  ctx);
            Emit(il, b.Right, ctx);

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

        EmitToStack(il, b.Left,  ctx);
        EmitToStack(il, b.Right, ctx);

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
        return typeof(object);
    }

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
        if (ctx.Builtins.TryGetValue(call.Callee, out var builtin))
        {
            foreach (var arg in call.Arguments)
                EmitToStack(il, arg, ctx);
            il.Emit(OpCodes.Call, builtin);
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

        if (ctx.Builtins.TryGetValue(key, out var builtin))
        {
            foreach (var arg in qCall.Arguments)
                EmitToStack(il, arg, ctx);
            il.Emit(OpCodes.Call, builtin);
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
