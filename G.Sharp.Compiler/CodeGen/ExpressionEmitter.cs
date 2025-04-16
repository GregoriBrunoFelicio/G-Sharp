using System.Reflection.Emit;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.CodeGen;

public static class ExpressionEmitter
{
    public static LocalBuilder Emit(ILGenerator il, Expression expression, Dictionary<string, LocalBuilder> locals)
    {
        return expression switch
        {
            LiteralExpression lit => EmitLiteralAndStore(il, lit.Value),
            VariableExpression v => EmitVariable(il, v.Name, locals),
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

            default:
                throw new NotSupportedException($"Unsupported expression type: {expression.GetType().Name}");
        }
    }

    private static LocalBuilder EmitLiteralAndStore(ILGenerator il, VariableValue value)
    {
        EmitLiteralToStack(il, value);
        var local = il.DeclareLocal(value.Type.GetClrType());
        il.Emit(OpCodes.Stloc, local);
        return local;
    }

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

    private static LocalBuilder EmitVariable(ILGenerator il, string name, Dictionary<string, LocalBuilder> locals)
    {
        if (!locals.TryGetValue(name, out var local))
            throw new Exception($"Variable '{name}' not found.");

        il.Emit(OpCodes.Ldloc, local);
        return local;
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