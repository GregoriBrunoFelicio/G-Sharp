using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;

namespace GSharp.CodeGen;

//TODO: TESTS
public static class IfEmitter
{
    public static void Emit(ILGenerator il, IfStatement ifStmt, Dictionary<string, LocalBuilder> locals)
    {
        var elseLabel = il.DefineLabel();
        var endLabel = il.DefineLabel();

        ExpressionEmitter.EmitToStack(il, ifStmt.Condition, locals);
        il.Emit(OpCodes.Call,
            typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsTrue))!);
        il.Emit(OpCodes.Brfalse, elseLabel);

        foreach (var stmt in ifStmt.ThenBody)
            StatementEmitter.Emit(il, stmt, locals);

        il.Emit(OpCodes.Br, endLabel);

        il.MarkLabel(elseLabel);
        if (ifStmt.ElseBody != null)
        {
            foreach (var stmt in ifStmt.ElseBody)
                StatementEmitter.Emit(il, stmt, locals);
        }

        il.MarkLabel(endLabel);
    }
}