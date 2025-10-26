using System.Reflection.Emit;
using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.CodeGen;

public static class ExpressionEmitter
{
    public static LocalBuilder Emit(ILGenerator il, Expression expression, Dictionary<string, LocalBuilder> locals)
    {
        return expression switch
        {
            LiteralExpression lit => EmitLiteralAndStore(il, lit.Value),
            VariableExpression v => EmitVariableAndStore(il, v.Name, locals),
            BinaryExpression b => EmitBinaryExpression(il, b, locals),
            _ => throw new NotSupportedException($"Unsupported expression type: {expression.GetType().Name}")
        };
    }

    public static void EmitToStack(ILGenerator il, Expression expression, Dictionary<string, LocalBuilder> locals)
    {
        switch (expression)
        {
            case LiteralExpression lit:
                EmitLiteralToStack(il, lit.Value);
                break;
            case VariableExpression v:
                if (!locals.TryGetValue(v.Name, out var local))
                    throw new Exception($"Variable '{v.Name}' not found.");
                il.Emit(OpCodes.Ldloc, local);
                break;
            case BinaryExpression b:
                EmitBinaryToStack(il, b, locals);
                break;
            default:
                throw new NotSupportedException($"Unsupported expression type: {expression.GetType().Name}");
        }
    }

    private static LocalBuilder EmitLiteralAndStore(ILGenerator il, VariableValue value)
    {
        EmitLiteralToStack(il, value);
        var local = il.DeclareLocal(GetClrTypeFromValue(value));

        il.Emit(OpCodes.Stloc, local);
        return local;
    }
    
    private static Type GetClrTypeFromValue(VariableValue value) => value switch
    {
        IntValue      => typeof(int),
        FloatValue    => typeof(float),
        DoubleValue   => typeof(double),
        DecimalValue  => typeof(decimal),
        BooleanValue  => typeof(bool),
        StringValue   => typeof(string),
        ArrayValue a  => a.ElementType.GetClrType().MakeArrayType(),
        _ => value.Type.GetClrType() 
    };

    private static void EmitLiteralToStack(ILGenerator il, VariableValue value)
    {
        switch (value)
        {
            case IntValue i:
                il.Emit(OpCodes.Ldc_I4, i.Value);
                break;
            case FloatValue f:
                il.Emit(OpCodes.Ldc_R4, f.Value);
                break;
            case DoubleValue d:
                il.Emit(OpCodes.Ldc_R8, d.Value);
                break;
            case DecimalValue m:
                EmitDecimalToStack(il, m.Value);
                break;
            case BooleanValue b:
                il.Emit(b.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                break;
            case StringValue s:
                il.Emit(OpCodes.Ldstr, s.Value);
                break;
            case ArrayValue a:
                ArrayEmitter.EmitToStack(il, a);
                break;
            default:
                throw new NotSupportedException($"Unsupported literal type: {value.GetType().Name}");
        }
    }

    private static LocalBuilder EmitVariableAndStore(ILGenerator il, string name, Dictionary<string, LocalBuilder> locals)
    {
        if (!locals.TryGetValue(name, out var local))
            throw new Exception($"Variable '{name}' not found.");

        il.Emit(OpCodes.Ldloc, local);
        var temp = il.DeclareLocal(local.LocalType);
        il.Emit(OpCodes.Stloc, temp);
        return temp;
    }

    private static LocalBuilder EmitBinaryExpression(ILGenerator il, BinaryExpression expr, Dictionary<string, LocalBuilder> locals)
    {
        if (expr.Operator is TokenType.And or TokenType.Or)
            return EmitLogicalShortCircuit(il, expr, locals);

        EmitToStack(il, expr.Left, locals);
        EmitToStack(il, expr.Right, locals);
        EmitBinaryOperator(il, expr.Operator);

        var result = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Stloc, result);
        return result;
    }

    private static void EmitBinaryToStack(ILGenerator il, BinaryExpression expr, Dictionary<string, LocalBuilder> locals)
    {
        if (expr.Operator is TokenType.And or TokenType.Or)
        {
            var result = EmitLogicalShortCircuit(il, expr, locals);
            il.Emit(OpCodes.Ldloc, result);
        }
        else
        {
            EmitToStack(il, expr.Left, locals);
            EmitToStack(il, expr.Right, locals);

            EmitBinaryOperator(il, expr.Operator);
        }
    }

    private static void EmitBinaryOperator(ILGenerator il, TokenType op)
    {
        switch (op)
        {
            case TokenType.GreaterThan:
                il.Emit(OpCodes.Cgt);
                break;
            case TokenType.LessThan:
                il.Emit(OpCodes.Clt);
                break;
            case TokenType.EqualEqual:
                il.Emit(OpCodes.Ceq);
                break;
            case TokenType.NotEqual:
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
                break;
            case TokenType.GreaterThanOrEqual:
                il.Emit(OpCodes.Clt);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
                break;
            case TokenType.LessThanOrEqual:
                il.Emit(OpCodes.Cgt);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
                break;
            case TokenType.Plus:
                il.Emit(OpCodes.Add);
                break;
            case TokenType.Minus:
                il.Emit(OpCodes.Sub);
                break;
            case TokenType.Multiply:
                il.Emit(OpCodes.Mul);
                break;
            case TokenType.Divide:
                il.Emit(OpCodes.Div);
                break;
            default:
                throw new NotSupportedException($"Unsupported binary operator: {op}");
        }
    }

    private static LocalBuilder EmitLogicalShortCircuit(ILGenerator il, BinaryExpression expr, Dictionary<string, LocalBuilder> locals)
    {
        var result = il.DeclareLocal(typeof(int));
        var shortCircuit = il.DefineLabel();
        var end = il.DefineLabel();

        EmitToStack(il, expr.Left, locals);

        if (expr.Operator == TokenType.And)
        {
            il.Emit(OpCodes.Brfalse, shortCircuit);
        }
        else
        {
            il.Emit(OpCodes.Brtrue, shortCircuit);
        }

        EmitToStack(il, expr.Right, locals);
        il.Emit(OpCodes.Stloc, result);
        il.Emit(OpCodes.Br, end);

        il.MarkLabel(shortCircuit);

        if (expr.Operator == TokenType.And)
            il.Emit(OpCodes.Ldc_I4_0);
        else
            il.Emit(OpCodes.Ldc_I4_1);

        il.Emit(OpCodes.Stloc, result);

        il.MarkLabel(end);
        // ATENÇÃO AQUI! NÃO FAZ Ldloc RESULT AQUI
        return result;
    }

    private static void EmitDecimalToStack(ILGenerator il, decimal value)
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
