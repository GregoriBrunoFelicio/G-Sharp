using System.Reflection.Emit;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.CodeGen;

public static class AssignmentEmitter
{
    public static void Emit(ILGenerator il, AssignmentStatement statement, Dictionary<string, LocalBuilder> locals)
    {
        if (!locals.TryGetValue(statement.VariableName, out var local))
            throw new InvalidOperationException($"Variable '{statement.VariableName}' is not defined.");

        EmitValueToStack(il, statement.Expression, locals);

        il.Emit(OpCodes.Stloc, local);
    }

    private static void EmitValueToStack(ILGenerator il, Expression expression, Dictionary<string, LocalBuilder> locals)
    {
        switch (expression)
        {
            case LiteralExpression literal:
                EmitLiteralToStack(il, literal.Value);
                break;

            case VariableExpression variable:
                if (!locals.TryGetValue(variable.Name, out var local))
                    throw new Exception($"Variable '{variable.Name}' not found.");
                il.Emit(OpCodes.Ldloc, local);
                break;

            default:
                throw new NotSupportedException($"Unsupported expression: {expression.GetType().Name}");
        }
    }

    private static void EmitLiteralToStack(ILGenerator il, VariableValue value)
    {
        switch (value)
        {
            case IntValue i: il.Emit(OpCodes.Ldc_I4, i.Value); break;
            case FloatValue f: il.Emit(OpCodes.Ldc_R4, f.Value); break;
            case DoubleValue d: il.Emit(OpCodes.Ldc_R8, d.Value); break;
            case BooleanValue b: il.Emit(b.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0); break;
            case StringValue s: il.Emit(OpCodes.Ldstr, s.Value); break;
            //TODO: add decimal support
            default: throw new NotSupportedException("Unsupported literal type");
        }
    }
}