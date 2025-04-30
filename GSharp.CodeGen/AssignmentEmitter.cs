using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

public static class AssignmentEmitter
{
    public static void Emit(ILGenerator il, AssignmentStatement statement, Dictionary<string, LocalBuilder> locals)
    {
        if (!locals.TryGetValue(statement.VariableName, out var local))
            throw new InvalidOperationException($"Variable '{statement.VariableName}' is not defined.");

        ExpressionEmitter.EmitToStack(il, statement.Expression, locals);

        il.Emit(OpCodes.Stloc, local);
    }
}
