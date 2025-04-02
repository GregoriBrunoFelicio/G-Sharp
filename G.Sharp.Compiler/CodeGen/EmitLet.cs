using System.Reflection.Emit;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.CodeGen;

public static class EmitLet
{
    public static void Emit(ILGenerator il, LetStatement statement, Dictionary<string, LocalBuilder> locals)
    {
        var local = statement.VariableValue switch
        {
            IntValue numberInt => EmitNumber.EmitInt(il, numberInt.Value),
            FloatValue numberFloat => EmitNumber.EmitFloat(il, numberFloat.Value),
            DoubleValue numberDouble => EmitNumber.EmitDouble(il, numberDouble.Value),
            DecimalValue numberDecimal => EmitDecimal.Emit(il, numberDecimal.Value),
            StringValue str => EmitString.Emit(il, str.Value),
            BooleanValue boolean => EmitBoolean.Emit(il, boolean.Value),
            ArrayValue array => EmitArray.Emit(il, array),
            _ => throw new NotSupportedException($"Unsupported value type: {statement.VariableValue.GetType().Name}")
        };

        locals[statement.VariableName] = local;
    }
}