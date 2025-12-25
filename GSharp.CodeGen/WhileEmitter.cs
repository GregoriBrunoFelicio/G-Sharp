using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;

namespace GSharp.CodeGen;

public static class WhileEmitter
{
    public static void Emit(ILGenerator il, WhileStatement whileStmt, Dictionary<string, LocalBuilder> locals)
    {
        var startLabel = il.DefineLabel();
        var endLabel = il.DefineLabel();

        il.MarkLabel(startLabel);

        ExpressionEmitter.EmitToStack(il, whileStmt.Condition, locals);
        il.Emit(OpCodes.Call,
            typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsTrue))!);
        il.Emit(OpCodes.Brfalse, endLabel);

        foreach (var stmt in whileStmt.Body)
            StatementEmitter.Emit(il, stmt, locals);

        il.Emit(OpCodes.Br, startLabel);
        il.MarkLabel(endLabel);
    }
}
