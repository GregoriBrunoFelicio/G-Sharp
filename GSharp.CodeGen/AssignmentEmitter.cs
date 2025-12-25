using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

public static class AssignmentEmitter
{
    public static void Emit(ILGenerator il, AssignmentStatement statement, Dictionary<string, LocalBuilder> locals)
    {
        if (!locals.TryGetValue(statement.VariableName, out var local))
            throw new Exception($"Variable '{statement.VariableName}' not found.");

        ExpressionEmitter.EmitToStack(il, statement.Expression, locals);
        il.Emit(OpCodes.Stloc, local);
    }
}
