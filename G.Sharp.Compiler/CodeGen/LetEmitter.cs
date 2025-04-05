using System.Reflection.Emit;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.CodeGen;

public static class LetEmitter
{
    public static void Emit(ILGenerator il, LetStatement statement, Dictionary<string, LocalBuilder> locals)
    {
        var local = statement.VariableValue switch
        {
            IntValue numberInt => NumberEmitter.EmitInt(il, numberInt.Value),
            FloatValue numberFloat => NumberEmitter.EmitFloat(il, numberFloat.Value),
            DoubleValue numberDouble => NumberEmitter.EmitDouble(il, numberDouble.Value),
            DecimalValue numberDecimal => DecimalEmitter.Emit(il, numberDecimal.Value),
            StringValue str => StringEmitter.Emit(il, str.Value),
            BooleanValue boolean => BooleanEmitter.Emit(il, boolean.Value),
            ArrayValue array => ArrayEmitter.Emit(il, array),
            _ => throw new NotSupportedException($"Unsupported value type: {statement.VariableValue.GetType().Name}")
        };

        locals[statement.VariableName] = local;
    }
}