using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;
using GSharp.Lexer;

namespace GSharp.CodeGen;

public static class ExpressionEmitter
{
    public static void EmitToStack(
        ILGenerator il,
        Expression expression,
        Dictionary<string, LocalBuilder> locals)
    {
        switch (expression)
        {
            case LiteralExpression lit:
                EmitLiteral(il, lit.Value);
                break;

            case VariableExpression v:
                il.Emit(OpCodes.Ldloc, locals[v.Name]);
                break;

            case BinaryExpression b:
                EmitBinary(il, b, locals);
                break;

            default:
                throw new NotSupportedException(
                    $"Unsupported expression: {expression.GetType().Name}");
        }
    }

    private static void EmitBinary(
        ILGenerator il,
        BinaryExpression expr,
        Dictionary<string, LocalBuilder> locals)
    {
        EmitToStack(il, expr.Left, locals);
        EmitToStack(il, expr.Right, locals);

        var method = expr.Operator switch
        {
            TokenType.GreaterThan =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.GreaterThan)),
            TokenType.LessThan =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.LessThan)),
            TokenType.GreaterThanOrEqual =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.GreaterThanOrEqual)),
            TokenType.LessThanOrEqual =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.LessThanOrEqual)),
            TokenType.EqualEqual =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.EqualEqual)),
            TokenType.NotEqual =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.NotEqual)),
            TokenType.Plus =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Add)),
            TokenType.Minus =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Subtract)),
            _ => throw new NotSupportedException(expr.Operator.ToString())
        };

        il.Emit(OpCodes.Call, method!);
        // resultado: object no topo da stack
    }

    private static void EmitLiteral(ILGenerator il, object value)
    {
        switch (value)
        {
            case int i:
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Box, typeof(int));
                break;

            case double d:
                il.Emit(OpCodes.Ldc_R8, d);
                il.Emit(OpCodes.Box, typeof(double));
                break;

            case bool b:
                il.Emit(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Box, typeof(bool));
                break;

            case string s:
                il.Emit(OpCodes.Ldstr, s);
                break;
            
            case float f:
                il.Emit(OpCodes.Ldc_R4, f);
                il.Emit(OpCodes.Box, typeof(float));
                break;
            
            case decimal m:
                EmitDecimal(il, m);
                il.Emit(OpCodes.Box, typeof(decimal));
                break;
            
            case object[] arr:
                ArrayEmitter.EmitToStack(il, arr);
                break;

            default:
                throw new NotSupportedException(
                    $"Unsupported literal: {value?.GetType().Name}");
        }
    }

    private static void EmitDecimal(ILGenerator il, decimal value)
    {
        var bits = decimal.GetBits(value);
        var lo = bits[0];
        var mid = bits[1];
        var hi = bits[2];
        var sign = (bits[3] & 0x80000000) != 0;
        var scale = (byte)((bits[3] >> 16) & 0x7F);

        var ctor = typeof(decimal).GetConstructor([
            typeof(int), typeof(int), typeof(int),
            typeof(bool), typeof(byte)
        ]) ?? throw new Exception("Decimal constructor not found");

        il.Emit(OpCodes.Ldc_I4, lo);
        il.Emit(OpCodes.Ldc_I4, mid);
        il.Emit(OpCodes.Ldc_I4, hi);
        il.Emit(sign ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ldc_I4_S, scale);
        il.Emit(OpCodes.Newobj, ctor);
    }
}