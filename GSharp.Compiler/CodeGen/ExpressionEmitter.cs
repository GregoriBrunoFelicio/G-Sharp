using System.Reflection;
using System.Reflection.Emit;
using GSharp.Compiler.AST;
using GSharp.Compiler.CodeGen.Helpers;
using GSharp.Compiler.Lexer;
using GSharp.Compiler.TypeChecker;

namespace GSharp.Compiler.CodeGen;

public static class ExpressionEmitter
{
    // -------------------------------------------------------------------------
    // Cached reflection — looked up once at class load, not on every emission
    // -------------------------------------------------------------------------

    private static readonly MethodInfo GsFunctionCallMethod =
        typeof(GSharpFunction).GetMethod(nameof(GSharpFunction.Call))!;

    private static readonly MethodInfo GsFunctionCall1Method =
        typeof(GSharpFunction).GetMethod(nameof(GSharpFunction.Call1))!;

    private static readonly MethodInfo GreaterThanMethod =
        typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.GreaterThan))!;

    private static readonly MethodInfo LessThanMethod =
        typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.LessThan))!;

    private static readonly MethodInfo GreaterThanOrEqualMethod =
        typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.GreaterThanOrEqual))!;

    private static readonly MethodInfo LessThanOrEqualMethod =
        typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.LessThanOrEqual))!;

    private static readonly MethodInfo EqualEqualMethod =
        typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.EqualEqual))!;

    private static readonly MethodInfo NotEqualMethod =
        typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.NotEqual))!;

    private static readonly MethodInfo AddMethod = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Add))!;

    private static readonly MethodInfo SubtractMethod =
        typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Subtract))!;

    private static readonly MethodInfo MultiplyMethod =
        typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Multiply))!;

    private static readonly MethodInfo DivideMethod = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Divide))!;
    private static readonly MethodInfo IsTrueMethod = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsTrue))!;

    private static readonly MethodInfo NegateMethod = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Negate))!;

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

    private static void EmitArgAs(ILGenerator il, Expression arg, Type expected, EmitContext context)
    {
        var actual = Emit(il, arg, context);
        if (actual == expected) return;

        if (expected.IsValueType && actual == typeof(object))
            il.Emit(OpCodes.Unbox_Any, expected);
        else if (!expected.IsValueType && actual.IsValueType)
            il.Emit(OpCodes.Box, actual);
    }

    private static Type Emit(ILGenerator il, Expression expression, EmitContext context)
    {
        switch (expression)
        {
            case LiteralExpression literal:
                return EmitLiteral(il, literal.Value);

            case IdentifierExpression binding:
                return EmitBinding(il, binding, context);

            case CallExpression call:
                EmitCall(il, call, context);
                if (!context.TypeMap.TryGetValue(call, out var callGsType)) return typeof(object);
                var returnClrType = GsTypeToClr(callGsType);
                if (returnClrType == typeof(object)) return typeof(object);
                il.Emit(OpCodes.Unbox_Any, returnClrType);
                return returnClrType;

            case ModuleCallExpression moduleCall:
                EmitModuleCall(il, moduleCall, context);
                if (!context.TypeMap.TryGetValue(moduleCall, out var mcGsType)) return typeof(object);
                var mcClrType = GsTypeToClr(mcGsType);
                if (mcClrType == typeof(object)) return typeof(object);
                il.Emit(OpCodes.Unbox_Any, mcClrType);
                return mcClrType;

            case BinaryExpression binary:
                return EmitBinary(il, binary, context);

            case UnaryExpression unary:
                return EmitUnary(il, unary, context);

            case BindingExpression binding:
                EmitBindingDeclaration(il, binding, context);
                return typeof(object);

            case PrintExpression printExpression:
                EmitPrint(il, printExpression, context);
                return typeof(object);

            case ForExpression forExpression:
                EmitFor(il, forExpression, context);
                return typeof(object[]);

            case IfExpression ifExpression:
                EmitIf(il, ifExpression, context);
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
                EmitArrayToStack(il, elements);
                return typeof(object[]);

            default:
                throw new NotSupportedException(
                    $"internal error: no emitter for literal type '{value?.GetType().Name ?? "null"}'");
        }
    }

    private static void EmitDecimalLiteral(ILGenerator il, decimal value)
    {
        var bits = decimal.GetBits(value);
        var lo = bits[0];
        var mid = bits[1];
        var hi = bits[2];
        var sign = (bits[3] & 0x80000000) != 0;
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

    private static void EmitBindingDeclaration(ILGenerator il, BindingExpression binding, EmitContext context)
    {
        var clrType = Emit(il, binding.Value, context);
        var local = il.DeclareLocal(clrType);
        il.Emit(OpCodes.Stloc, local);
        context.Locals[binding.BindingName] = local;
        il.Emit(OpCodes.Ldnull);
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
            Emit(il, binary.Left, context);
            Emit(il, binary.Right, context);

            var opcode = binary.Operator switch
            {
                TokenType.Plus => OpCodes.Add,
                TokenType.Minus => OpCodes.Sub,
                TokenType.Multiply => OpCodes.Mul,
                TokenType.Divide => OpCodes.Div,
                _ => throw new InvalidOperationException(
                    $"internal error: '{binary.Operator}' is not a native arithmetic operator")
            };

            il.Emit(opcode);
            return nativeType;
        }

        EmitToStack(il, binary.Left, context);
        EmitToStack(il, binary.Right, context);

        var method = binary.Operator switch
        {
            TokenType.GreaterThan => GreaterThanMethod,
            TokenType.LessThan => LessThanMethod,
            TokenType.GreaterThanOrEqual => GreaterThanOrEqualMethod,
            TokenType.LessThanOrEqual => LessThanOrEqualMethod,
            TokenType.EqualEqual => EqualEqualMethod,
            TokenType.NotEqual => NotEqualMethod,
            TokenType.Plus => AddMethod,
            TokenType.Minus => SubtractMethod,
            TokenType.Multiply => MultiplyMethod,
            TokenType.Divide => DivideMethod,
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

        if (!context.TypeMap.TryGetValue(binary.Left, out var leftGsType) ||
            !context.TypeMap.TryGetValue(binary.Right, out var rightGsType))
            return null;

        if (!WillEmitNativeValue(binary.Left, context) || !WillEmitNativeValue(binary.Right, context))
            return null;

        return (leftGsType, rightGsType) switch
        {
            (IntType, IntType) => typeof(int),
            (FloatType, FloatType) => typeof(float),
            (DoubleType, DoubleType) => typeof(double),
            _ => null
        };
    }

    private static bool WillEmitNativeValue(Expression expression, EmitContext context)
    {
        return expression switch
        {
            LiteralExpression literal => literal.Value is int or float or double or bool or decimal,
            IdentifierExpression id => (context.Locals.TryGetValue(id.Name, out var local) &&
                                        local.LocalType.IsValueType)
                                       || (context.Parameters.TryGetValue(id.Name, out var param) &&
                                           param.ClrType.IsValueType),
            BinaryExpression nested => TryGetNativeArithmeticType(nested, context) is not null,
            UnaryExpression unary => TryGetNativeUnaryType(unary, context) is not null,
            CallExpression call => context.TypeMap.TryGetValue(call, out var t)
                                   && GsTypeToClr(t) != typeof(object),
            ModuleCallExpression mc => context.TypeMap.TryGetValue(mc, out var t)
                                       && GsTypeToClr(t) != typeof(object),
            _ => false
        };
    }

    // -------------------------------------------------------------------------
    // Unary emission
    // -------------------------------------------------------------------------

    private static Type EmitUnary(ILGenerator il, UnaryExpression unary, EmitContext context)
    {
        if (unary.Operator == TokenType.Not)
        {
            var operandType = Emit(il, unary.Operand, context);
            if (operandType != typeof(bool))
                il.Emit(OpCodes.Unbox_Any, typeof(bool));
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
            return typeof(bool);
        }

        var nativeType = TryGetNativeUnaryType(unary, context);
        if (nativeType is not null)
        {
            Emit(il, unary.Operand, context);
            il.Emit(OpCodes.Neg);
            return nativeType;
        }

        EmitToStack(il, unary.Operand, context);
        il.Emit(OpCodes.Call, NegateMethod);
        return typeof(object);
    }

    private static Type? TryGetNativeUnaryType(UnaryExpression unary, EmitContext context)
    {
        if (unary.Operator != TokenType.Minus)
            return null;

        if (!context.TypeMap.TryGetValue(unary.Operand, out var operandType))
            return null;

        if (!WillEmitNativeValue(unary.Operand, context))
            return null;

        return operandType switch
        {
            IntType => typeof(int),
            FloatType => typeof(float),
            DoubleType => typeof(double),
            _ => null
        };
    }

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
        {
            throw new Exception($"Undefined function: '{key}'");
        }
    }

    // -------------------------------------------------------------------------
    // Short-circuit AND / OR
    // -------------------------------------------------------------------------

    private static LocalBuilder EmitLogicalShortCircuit(
        ILGenerator il, BinaryExpression binary, EmitContext context)
    {
        var result = il.DeclareLocal(typeof(bool));

        var shortCircuit = il.DefineLabel();
        var end = il.DefineLabel();

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
        var funcType = typeof(Func<object[], object>);
        var funcCtor = funcType.GetConstructors()[0];
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

    // -------------------------------------------------------------------------
    // Array literal emission
    // -------------------------------------------------------------------------

    private static void EmitArrayToStack(ILGenerator il, object[] array)
    {
        il.Emit(OpCodes.Ldc_I4, array.Length);
        il.Emit(OpCodes.Newarr, typeof(object));
        for (var i = 0; i < array.Length; i++)
        {
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4, i);
            EmitArrayElement(il, array[i]);
            il.Emit(OpCodes.Stelem_Ref);
        }
    }

    private static void EmitArrayElement(ILGenerator il, object value)
    {
        switch (value)
        {
            case int intValue:
                il.Emit(OpCodes.Ldc_I4, intValue);
                il.Emit(OpCodes.Box, typeof(int));
                break;
            case double doubleValue:
                il.Emit(OpCodes.Ldc_R8, doubleValue);
                il.Emit(OpCodes.Box, typeof(double));
                break;
            case bool boolValue:
                il.Emit(boolValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Box, typeof(bool));
                break;
            case string text:
                il.Emit(OpCodes.Ldstr, text);
                break;
            default:
                throw new NotSupportedException(
                    $"internal error: no emitter for array element type '{value?.GetType().Name ?? "null"}'");
        }
    }

    // -------------------------------------------------------------------------
    // If emission
    // -------------------------------------------------------------------------

    private static void EmitIf(ILGenerator il, IfExpression ifExpression, EmitContext context)
    {
        var elseLabel = il.DefineLabel();
        var endLabel = il.DefineLabel();

        var condType = Emit(il, ifExpression.Condition, context);
        if (condType == typeof(bool))
        {
            il.Emit(OpCodes.Brfalse, elseLabel);
        }
        else
        {
            il.Emit(OpCodes.Call, IsTrueMethod);
            il.Emit(OpCodes.Brfalse, elseLabel);
        }

        EmitIfBody(il, ifExpression.ThenBody, context);

        il.Emit(OpCodes.Br, endLabel);

        il.MarkLabel(elseLabel);

        if (ifExpression.ElseBody is { Count: > 0 })
            EmitIfBody(il, ifExpression.ElseBody, context);
        else
            il.Emit(OpCodes.Ldnull);
        il.MarkLabel(endLabel);
    }

    private static void EmitIfBody(ILGenerator il, List<Expression> body, EmitContext context)
    {
        if (body.Count == 0)
        {
            il.Emit(OpCodes.Ldnull);
            return;
        }

        foreach (var expr in body.SkipLast(1))
        {
            EmitToStack(il, expr, context);
            il.Emit(OpCodes.Pop);
        }

        EmitToStack(il, body[^1], context);
    }

    // -------------------------------------------------------------------------
    // For loop emission
    // -------------------------------------------------------------------------

    private static void EmitFor(ILGenerator il, ForExpression forExpression, EmitContext context)
    {
        EmitToStack(il, forExpression.Iterable, context);
        il.Emit(OpCodes.Castclass, typeof(object[]));
        var arrayLocal = il.DeclareLocal(typeof(object[]));
        il.Emit(OpCodes.Stloc, arrayLocal);

        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Conv_I4);
        il.Emit(OpCodes.Newarr, typeof(object));
        var resultLocal = il.DeclareLocal(typeof(object[]));
        il.Emit(OpCodes.Stloc, resultLocal);

        var indexLocal = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc, indexLocal);

        var loopStart = il.DefineLabel();
        var loopEnd = il.DefineLabel();

        il.MarkLabel(loopStart);

        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Conv_I4);
        il.Emit(OpCodes.Bge, loopEnd);

        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldelem_Ref);

        var elementClrType = ResolveForElementClrType(forExpression, context);
        if (elementClrType != typeof(object))
            il.Emit(OpCodes.Unbox_Any, elementClrType);

        var loopVar = il.DeclareLocal(elementClrType);
        il.Emit(OpCodes.Stloc, loopVar);
        context.Locals[forExpression.BindingName] = loopVar;

        for (var i = 0; i < forExpression.Body.Count - 1; i++)
        {
            EmitToStack(il, forExpression.Body[i], context);
            il.Emit(OpCodes.Pop);
        }

        il.Emit(OpCodes.Ldloc, resultLocal);
        il.Emit(OpCodes.Ldloc, indexLocal);
        EmitToStack(il, forExpression.Body[^1], context);
        il.Emit(OpCodes.Stelem_Ref);

        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc, indexLocal);
        il.Emit(OpCodes.Br, loopStart);

        il.MarkLabel(loopEnd);

        il.Emit(OpCodes.Ldloc, resultLocal);
    }

    private static Type ResolveForElementClrType(ForExpression forExpression, EmitContext context)
    {
        if (!context.TypeMap.TryGetValue(forExpression.Iterable, out var gsType))
            return typeof(object);

        return gsType is ArrayType arrayType
            ? GsTypeToClr(arrayType.ElementType)
            : typeof(object);
    }

    // -------------------------------------------------------------------------
    // Print emission
    // -------------------------------------------------------------------------

    private static void EmitPrint(ILGenerator il, PrintExpression printExpression, EmitContext context)
    {
        EmitToStack(il, printExpression.Value, context);
        var method = typeof(Console)
            .GetMethod("WriteLine", [typeof(object)])!;
        il.Emit(OpCodes.Call, method);
        il.Emit(OpCodes.Ldnull);
    }

    // -------------------------------------------------------------------------
    // Tail call emission
    // -------------------------------------------------------------------------

    private static void EmitTail(ILGenerator il, Expression expression, EmitContext context)
    {
        if (context.TailCall is { } tco)
        {
            if (TryEmitTailCall(il, expression, tco, context))
                return;

            if (expression is IfExpression ifExpression)
            {
                EmitTailIf(il, ifExpression, context);
                return;
            }
        }

        EmitToStack(il, expression, context);
    }

    private static bool TryEmitTailCall(
        ILGenerator il, Expression expression, TailCallInfo tco, EmitContext context)
    {
        if (expression is not CallExpression call) return false;
        if (call.Callee != tco.FunctionName) return false;
        if (call.Arguments.Count != tco.ParameterCount) return false;

        var paramTypes = context.FunctionParamTypes.GetValueOrDefault(tco.FunctionName);
        for (var i = 0; i < call.Arguments.Count; i++)
        {
            var expected = paramTypes is not null && i < paramTypes.Length ? paramTypes[i] : typeof(object);
            EmitArgAs(il, call.Arguments[i], expected, context);
        }

        for (var i = tco.ParameterCount - 1; i >= 0; i--)
            il.Emit(OpCodes.Starg_S, (byte)i);

        il.Emit(OpCodes.Br, tco.StartLabel);
        return true;
    }

    private static void EmitTailIf(ILGenerator il, IfExpression ifExpression, EmitContext context)
    {
        var elseLabel = il.DefineLabel();
        var endLabel = il.DefineLabel();

        var condType = Emit(il, ifExpression.Condition, context);
        if (condType == typeof(bool))
        {
            il.Emit(OpCodes.Brfalse, elseLabel);
        }
        else
        {
            il.Emit(OpCodes.Call, IsTrueMethod);
            il.Emit(OpCodes.Brfalse, elseLabel);
        }

        EmitTailBody(il, ifExpression.ThenBody, context);
        il.Emit(OpCodes.Br, endLabel);

        il.MarkLabel(elseLabel);
        if (ifExpression.ElseBody is { Count: > 0 })
            EmitTailBody(il, ifExpression.ElseBody, context);
        else
            il.Emit(OpCodes.Ldnull);

        il.MarkLabel(endLabel);
    }

    private static void EmitTailBody(ILGenerator il, List<Expression> body, EmitContext context)
    {
        for (var i = 0; i < body.Count - 1; i++)
        {
            EmitToStack(il, body[i], context);
            il.Emit(OpCodes.Pop);
        }

        if (body.Count > 0)
            EmitTail(il, body[^1], context);
        else
            il.Emit(OpCodes.Ldnull);
    }

    // -------------------------------------------------------------------------
    // Function definition & emission — two-pass: Define registers every
    // MethodBuilder up front (so forward calls/recursion resolve), Emit fills
    // in the bodies afterward.
    // -------------------------------------------------------------------------

    internal static void DefineFunction(
        TypeBuilder typeBuilder,
        FunctionDeclaration fn,
        Dictionary<string, MethodBuilder> functions,
        Dictionary<string, MethodBuilder> adapters,
        Dictionary<Expression, GsType>? typeMap = null,
        string prefix = "",
        Dictionary<string, Type[]>? functionParamTypes = null,
        Dictionary<string, MethodBuilder>? adapters1 = null,
        Dictionary<string, FieldBuilder>? functionFields = null)
    {
        var qualifiedName = prefix + fn.Name;
        var paramTypes = ResolveParameterClrTypes(fn, typeMap);

        var method = typeBuilder.DefineMethod(
            qualifiedName,
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(object),
            paramTypes);

        functions[qualifiedName] = method;
        functionParamTypes?[qualifiedName] = paramTypes;

        var adapter = typeBuilder.DefineMethod(
            qualifiedName + "__adapter",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(object),
            [typeof(object[])]);

        adapters[qualifiedName] = adapter;

        // Arity-1 adapter: single object parameter, no array allocation at call sites.
        if (fn.Parameters.Count == 1 && adapters1 is not null)
        {
            var adapter1 = typeBuilder.DefineMethod(
                qualifiedName + "__adapter1",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(object),
                [typeof(object)]);
            adapters1[qualifiedName] = adapter1;
        }

        if (functionFields is not null)
        {
            var field = typeBuilder.DefineField(
                "__fn_" + qualifiedName,
                typeof(GSharpFunction),
                FieldAttributes.Public | FieldAttributes.Static);
            functionFields[qualifiedName] = field;
        }
    }

    internal static void EmitFunction(FunctionDeclaration fn, EmitContext context, string prefix = "")
    {
        EmitFunctionMainBody(fn, context, prefix);
        EmitFunctionAdapter(fn, context, prefix);

        var qualifiedName = prefix + fn.Name;
        if (context.FunctionAdapters1.ContainsKey(qualifiedName))
            EmitFunctionAdapter1(fn, context, prefix);
    }

    private static void EmitFunctionMainBody(FunctionDeclaration fn, EmitContext context, string prefix)
    {
        var qualifiedName = prefix + fn.Name;
        var method = context.Functions[qualifiedName];
        var il = method.GetILGenerator();

        var functionContext = new EmitContext(
            context.Functions,
            context.FunctionAdapters,
            context.TypeMap,
            context.FunctionParamTypes,
            context.FunctionAdapters1,
            context.FunctionFields);
        foreach (var (builtinName, builtinMethod) in context.Builtins)
            functionContext.Builtins[builtinName] = builtinMethod;

        var paramClrTypes = ResolveParameterClrTypes(fn, context.TypeMap);
        for (var i = 0; i < fn.Parameters.Count; i++)
            functionContext.Parameters[fn.Parameters[i]] = (i, paramClrTypes[i]);

        var startLabel = il.DefineLabel();
        il.MarkLabel(startLabel);
        functionContext.TailCall = new TailCallInfo(qualifiedName, fn.Parameters.Count, startLabel);

        var body = fn.Body;

        for (var i = 0; i < body.Count - 1; i++)
        {
            EmitToStack(il, body[i], functionContext);
            il.Emit(OpCodes.Pop);
        }

        if (body.Count > 0)
            EmitTail(il, body[^1], functionContext);
        else
            il.Emit(OpCodes.Ldnull);

        il.Emit(OpCodes.Ret);
    }

    private static void EmitFunctionAdapter(FunctionDeclaration fn, EmitContext context, string prefix)
    {
        var qualifiedName = prefix + fn.Name;
        var adapter = context.FunctionAdapters[qualifiedName];
        var il = adapter.GetILGenerator();

        var paramClrTypes = ResolveParameterClrTypes(fn, context.TypeMap);
        for (var i = 0; i < fn.Parameters.Count; i++)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, i);
            il.Emit(OpCodes.Ldelem_Ref);
            if (paramClrTypes[i].IsValueType)
                il.Emit(OpCodes.Unbox_Any, paramClrTypes[i]);
        }

        il.Emit(OpCodes.Call, context.Functions[qualifiedName]);
        il.Emit(OpCodes.Ret);
    }

    // Single-object adapter for arity-1 functions: avoids array unpacking.
    private static void EmitFunctionAdapter1(FunctionDeclaration fn, EmitContext context, string prefix)
    {
        var qualifiedName = prefix + fn.Name;
        var adapter1 = context.FunctionAdapters1[qualifiedName];
        var il = adapter1.GetILGenerator();

        var paramClrTypes = ResolveParameterClrTypes(fn, context.TypeMap);
        il.Emit(OpCodes.Ldarg_0);
        if (paramClrTypes[0].IsValueType)
            il.Emit(OpCodes.Unbox_Any, paramClrTypes[0]);

        il.Emit(OpCodes.Call, context.Functions[qualifiedName]);
        il.Emit(OpCodes.Ret);
    }

    private static Type[] ResolveParameterClrTypes(FunctionDeclaration fn, Dictionary<Expression, GsType>? typeMap)
    {
        if (typeMap is null || !typeMap.TryGetValue(fn, out var fnType))
            return Enumerable.Repeat(typeof(object), fn.Parameters.Count).ToArray();

        var clrTypes = new Type[fn.Parameters.Count];
        var currentType = fnType;
        for (var i = 0; i < fn.Parameters.Count; i++)
        {
            clrTypes[i] = currentType is FunctionType ft ? GsTypeToClr(ft.ParameterType) : typeof(object);
            currentType = currentType is FunctionType ft2 ? ft2.ReturnType : currentType;
        }

        return clrTypes;
    }

    private static Type GsTypeToClr(GsType type)
    {
        return type switch
        {
            IntType => typeof(int),
            FloatType => typeof(float),
            DoubleType => typeof(double),
            DecimalType => typeof(decimal),
            BoolType => typeof(bool),
            _ => typeof(object)
        };
    }
}