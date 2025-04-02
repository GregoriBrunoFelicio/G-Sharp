using System.Reflection.Emit;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.CodeGen;

public static class EmitAssignment
{
    public static void Emit(ILGenerator il, AssignmentStatement statement, Dictionary<string, LocalBuilder> locals)
    {
        if (!locals.TryGetValue(statement.VariableName, out var local))
            throw new InvalidOperationException($"Variable '{statement.VariableName}' is not defined.");

        switch (statement.VariableValue)
        {
            case IntValue intVal:
                il.Emit(OpCodes.Ldc_I4, intVal.Value);
                break;

            case FloatValue floatVal:
                il.Emit(OpCodes.Ldc_R4, floatVal.Value);
                break;

            case DoubleValue doubleVal:
                il.Emit(OpCodes.Ldc_R8, doubleVal.Value);
                break;

            case DecimalValue decVal:
                EmitDecimal.Emit(il, decVal.Value);
                return;

            case StringValue strVal:
                il.Emit(OpCodes.Ldstr, strVal.Value);
                break;

            case BooleanValue boolVal:
                il.Emit(boolVal.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                break;

            default:
                throw new NotSupportedException($"Unsupported value type: {statement.VariableValue.GetType().Name}");
        }

        il.Emit(OpCodes.Stloc, local);
    }
}