using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;
using GSharp.Lexer;
using GSharp.TypeChecker;

namespace GSharp.CodeGen;

public static class ExpressionEmitter
{
    // -------------------------------------------------------------------------
    // Cached reflection — looked up once at class load, not on every emission
    // -------------------------------------------------------------------------

    private static readonly MethodInfo GsFunctionCallMethod =
        typeof(GSharpFunction).GetMethod(nameof(GSharpFunction.Call))!;
    private static readonly MethodInfo GsFunctionCall1Method =
        typeof(GSharpFunction).GetMethod(nameof(GSharpFunction.Call1))!;

    private static readonly MethodInfo GreaterThanMethod        = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.GreaterThan))!;
    private static readonly MethodInfo LessThanMethod           = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.LessThan))!;
    private static readonly MethodInfo GreaterThanOrEqualMethod = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.GreaterThanOrEqual))!;
    private static readonly MethodInfo LessThanOrEqualMethod    = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.LessThanOrEqual))!;
    private static readonly MethodInfo EqualEqualMethod         = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.EqualEqual))!;
    private static readonly MethodInfo NotEqualMethod           = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.NotEqual))!;
    private static readonly MethodInfo AddMethod                = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Add))!;
    private static readonly MethodInfo SubtractMethod           = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Subtract))!;
    private static readonly MethodInfo MultiplyMethod           = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Multiply))!;
    private static readonly MethodInfo DivideMethod             = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Divide))!;

    private static readonly ConstructorInfo DecimalCtor = typeof(decimal).GetConstructor([
        typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte)
    ]) ?? throw new Exception("Decimal constructor not found");

    // -------------------------------------------------------------------------
    // Public entry points
    // -------------------------------------------------------------------------

    public static void EmitToStack(ILGenerator il, Expression expression, EmitContext context)
    {
        var clrType = Emit(il, expression, context);
        if (clrType.IsValueType)
            il.Emit(OpCodes.Box, clrType);
    }

    internal static void EmitArgAs(ILGenerator il, Expression arg, Type expected, EmitContext context)
    {
        var actual = Emit(il, arg, context);
        if (actual == expected) return;

        if (expected.IsValueType && actual == typeof(object))
            il.Emit(OpCodes.Unbox_Any, expected);
        else if (!expected.IsValueType && actual.IsValueType)
            il.Emit(OpCodes.Box, actual);
    }

    internal static Type Emit(ILGenerator il, Expression expression, EmitContext context)
    {
        switch (expression)
        {
            case LiteralExpression literal:
                return EmitLiteral(il, literal.Value);

            case IdentifierExpression binding:
                return EmitBinding(il, binding, context);

            case CallExpression call:
                EmitCall(il, call, context);
                if (context.TypeMap.TryGetValue(call, out var callGsType))
                {
                    var returnClrType = FunctionEmitter.GsTypeToClr(callGsType);
                    if (returnClrType != typeof(object))
                    {
                        il.Emit(OpCodes.Unbox_Any, returnClrType);
                        return returnClrType;
                    }
                }
                return typeof(object);

            case ModuleCallExpression moduleCall:
                EmitModuleCall(il, moduleCall, context);
                if (context.TypeMap.TryGetValue(moduleCall, out var mcGsType))
                {
                    var mcClrType = FunctionEmitter.GsTypeToClr(mcGsType);
                    if (mcClrType != typeof(object))
                    {
                        il.Emit(OpCodes.Unbox_Any, mcClrType);
                        return mcClrType;
                    }
                }
                return typeof(object);

            case BinaryExpression binary:
                return EmitBinary(il, binary, context);

            case BindingExpression binding:
                BindingEmitter.Emit(il, binding, context);
                return typeof(object);

            case PrintExpression printExpression:
                PrintEmitter.Emit(il, printExpression, context);
                return typeof(object);

            case ForExpression forExpression:
                ForEmitter.Emit(il, forExpression, context);
                return typeof(object[]);

            case IfExpression ifExpression:
                IfEmitter.EmitToStack(il, ifExpression, context);
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
            case int intValue:
                il.Emit(OpCodes.Ldc_I4, intValue);
                return typeof(int);

            case double doubleValue:
                il.Emit(OpCodes.Ldc_R8, doubleValue);
                return typeof(double);

            case bool boolValue:
                il.Emit(boolValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                return typeof(bool);

            case string text:
                il.Emit(OpCodes.Ldstr, text);
                return typeof(string);

            case float floatValue:
                il.Emit(OpCodes.Ldc_R4, floatValue);
                return typeof(float);

            case decimal decimalValue:
                EmitDecimalLiteral(il, decimalValue);
                return typeof(decimal);

            case object[] elements:
                ArrayEmitter.EmitToStack(il, elements);
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

        il.Emit(OpCodes.Ldc_I4, lo);
        il.Emit(OpCodes.Ldc_I4, mid);
        il.Emit(OpCodes.Ldc_I4, hi);
        il.Emit(sign ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ldc_I4_S, scale);
        il.Emit(OpCodes.Newobj, DecimalCtor);
    }

    // -------------------------------------------------------------------------
    // Binding emission
    // -------------------------------------------------------------------------

    private static Type EmitBinding(ILGenerator il, IdentifierExpression binding, EmitContext context)
    {
        if (context.Parameters.TryGetValue(binding.Name, out var param))
        {
            il.Emit(OpCodes.Ldarg, param.Index);
            return param.ClrType;
        }

        if (context.Locals.TryGetValue(binding.Name, out var local))
        {
            il.Emit(OpCodes.Ldloc, local);
            return local.LocalType;
        }

        // Load cached static GSharpFunction field (preferred — no allocation).
        if (context.FunctionFields.TryGetValue(binding.Name, out var field))
        {
            il.Emit(OpCodes.Ldsfld, field);
            return typeof(object);
        }

        // Fallback: allocate function wrapper on the spot (e.g. module functions).
        if (context.FunctionAdapters.TryGetValue(binding.Name, out var adapter))
        {
            EmitFunctionValue(il, adapter);
            return typeof(object);
        }

        throw new Exception($"Undefined binding: '{binding.Name}'");
    }

    // -------------------------------------------------------------------------
    // Binary emission
    // -------------------------------------------------------------------------

    private static Type EmitBinary(ILGenerator il, BinaryExpression binary, EmitContext context)
    {
        if (binary.Operator is TokenType.And or TokenType.Or)
        {
            var result = EmitLogicalShortCircuit(il, binary, context);
            il.Emit(OpCodes.Ldloc, result);
            return typeof(bool);
        }

        var nativeType = TryGetNativeArithmeticType(binary, context);
        if (nativeType is not null)
        {
            Emit(il, binary.Left,  context);
            Emit(il, binary.Right, context);

            var opcode = binary.Operator switch
            {
                TokenType.Plus     => OpCodes.Add,
                TokenType.Minus    => OpCodes.Sub,
                TokenType.Multiply => OpCodes.Mul,
                TokenType.Divide   => OpCodes.Div,
                _ => throw new InvalidOperationException(
                    $"internal error: '{binary.Operator}' is not a native arithmetic operator")
            };

            il.Emit(opcode);
            return nativeType;
        }

        EmitToStack(il, binary.Left,  context);
        EmitToStack(il, binary.Right, context);

        var method = binary.Operator switch
        {
            TokenType.GreaterThan        => GreaterThanMethod,
            TokenType.LessThan           => LessThanMethod,
            TokenType.GreaterThanOrEqual => GreaterThanOrEqualMethod,
            TokenType.LessThanOrEqual    => LessThanOrEqualMethod,
            TokenType.EqualEqual         => EqualEqualMethod,
            TokenType.NotEqual           => NotEqualMethod,
            TokenType.Plus               => AddMethod,
            TokenType.Minus              => SubtractMethod,
            TokenType.Multiply           => MultiplyMethod,
            TokenType.Divide             => DivideMethod,
            _ => throw new NotSupportedException(
                $"internal error: no emitter for binary operator '{binary.Operator}'")
        };

        il.Emit(OpCodes.Call, method);
        return typeof(object);
    }

    private static Type? TryGetNativeArithmeticType(BinaryExpression binary, EmitContext context)
    {
        if (binary.Operator is not (TokenType.Plus or TokenType.Minus or TokenType.Multiply or TokenType.Divide))
            return null;

        if (!context.TypeMap.TryGetValue(binary.Left,  out var leftGsType) ||
            !context.TypeMap.TryGetValue(binary.Right, out var rightGsType))
            return null;

        if (!WillEmitNativeValue(binary.Left, context) || !WillEmitNativeValue(binary.Right, context))
            return null;

        return (leftGsType, rightGsType) switch
        {
            (IntType,    IntType)    => typeof(int),
            (FloatType,  FloatType)  => typeof(float),
            (DoubleType, DoubleType) => typeof(double),
            _ => null
        };
    }

    private static bool WillEmitNativeValue(Expression expression, EmitContext context) => expression switch
    {
        LiteralExpression literal    => literal.Value is int or float or double or bool or decimal,
        IdentifierExpression id      => (context.Locals.TryGetValue(id.Name, out var local) && local.LocalType.IsValueType)
                                     || (context.Parameters.TryGetValue(id.Name, out var param) && param.ClrType.IsValueType),
        BinaryExpression nested      => TryGetNativeArithmeticType(nested, context) is not null,
        CallExpression call          => context.TypeMap.TryGetValue(call, out var t)
                                     && FunctionEmitter.GsTypeToClr(t) != typeof(object),
        ModuleCallExpression mc      => context.TypeMap.TryGetValue(mc, out var t)
                                     && FunctionEmitter.GsTypeToClr(t) != typeof(object),
        _ => false
    };

    // -------------------------------------------------------------------------
    // Call emission
    // -------------------------------------------------------------------------

    private static void EmitCall(ILGenerator il, CallExpression call, EmitContext context)
    {
        if (context.Builtins.TryGetValue(call.Callee, out var builtin))
        {
            foreach (var arg in call.Arguments)
                EmitToStack(il, arg, context);
            il.Emit(OpCodes.Call, builtin);
        }
        else if (context.Functions.TryGetValue(call.Callee, out var method))
        {
            var paramTypes = context.FunctionParamTypes.GetValueOrDefault(call.Callee);
            for (var i = 0; i < call.Arguments.Count; i++)
            {
                var expected = paramTypes is not null && i < paramTypes.Length ? paramTypes[i] : typeof(object);
                EmitArgAs(il, call.Arguments[i], expected, context);
            }
            il.Emit(OpCodes.Call, method);
        }
        else
        {
            EmitDelegateCall(il, call, context);
        }
    }

    private static void EmitModuleCall(ILGenerator il, ModuleCallExpression moduleCall, EmitContext context)
    {
        var key = $"{moduleCall.Module}.{moduleCall.Function}";

        if (context.Builtins.TryGetValue(key, out var builtin))
        {
            foreach (var arg in moduleCall.Arguments)
                EmitToStack(il, arg, context);
            il.Emit(OpCodes.Call, builtin);
        }
        else if (context.Functions.TryGetValue(key, out var moduleMethod))
        {
            foreach (var arg in moduleCall.Arguments)
                EmitToStack(il, arg, context);
            il.Emit(OpCodes.Call, moduleMethod);
        }
        else
            throw new Exception($"Undefined function: '{key}'");
    }

    // -------------------------------------------------------------------------
    // Short-circuit AND / OR
    // -------------------------------------------------------------------------

    private static LocalBuilder EmitLogicalShortCircuit(
        ILGenerator il, BinaryExpression binary, EmitContext context)
    {
        var result = il.DeclareLocal(typeof(bool));

        var shortCircuit = il.DefineLabel();
        var end          = il.DefineLabel();

        var leftType = Emit(il, binary.Left, context);
        if (leftType != typeof(bool))
            il.Emit(OpCodes.Unbox_Any, typeof(bool));
        il.Emit(binary.Operator == TokenType.And ? OpCodes.Brfalse : OpCodes.Brtrue, shortCircuit);

        var rightType = Emit(il, binary.Right, context);
        if (rightType != typeof(bool))
            il.Emit(OpCodes.Unbox_Any, typeof(bool));
        il.Emit(OpCodes.Stloc, result);
        il.Emit(OpCodes.Br, end);

        il.MarkLabel(shortCircuit);
        il.Emit(binary.Operator == TokenType.And ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
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

    private static void EmitDelegateCall(ILGenerator il, CallExpression call, EmitContext context)
    {
        if (context.Parameters.TryGetValue(call.Callee, out var param))
            il.Emit(OpCodes.Ldarg, param.Index);
        else if (context.Locals.TryGetValue(call.Callee, out var local))
            il.Emit(OpCodes.Ldloc, local);
        else
            throw new Exception($"Undefined function or binding: '{call.Callee}'");

        il.Emit(OpCodes.Castclass, typeof(GSharpFunction));

        if (call.Arguments.Count == 1)
        {
            EmitToStack(il, call.Arguments[0], context);
            il.Emit(OpCodes.Callvirt, GsFunctionCall1Method);
        }
        else
        {
            il.Emit(OpCodes.Ldc_I4, call.Arguments.Count);
            il.Emit(OpCodes.Newarr, typeof(object));

            for (var i = 0; i < call.Arguments.Count; i++)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i);
                EmitToStack(il, call.Arguments[i], context);
                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Callvirt, GsFunctionCallMethod);
        }
    }
}
