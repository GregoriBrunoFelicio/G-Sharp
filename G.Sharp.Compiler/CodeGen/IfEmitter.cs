using System.Reflection.Emit;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.CodeGen;

//TODO: TESTS
public static class IfEmitter
{
    public static void Emit(ILGenerator il, IfStatement ifStmt, Dictionary<string, LocalBuilder> locals)
    {
        var elseLabel = il.DefineLabel();
        var endLabel = il.DefineLabel();

        var conditionLocal = ExpressionEmitter.Emit(il, ifStmt.Condition, locals);

        il.Emit(OpCodes.Ldloc, conditionLocal);

        il.Emit(OpCodes.Brfalse, elseLabel);

        foreach (var stmt in ifStmt.ThenBody)
        {
            StatementEmitter.Emit(il, stmt, locals);
        }

        il.Emit(OpCodes.Br, endLabel);

        il.MarkLabel(elseLabel);
        if (ifStmt.ElseBody is not null)
        {
            foreach (var stmt in ifStmt.ElseBody)
            {
                StatementEmitter.Emit(il, stmt, locals);
            }
        }

        il.MarkLabel(endLabel);
    }
}