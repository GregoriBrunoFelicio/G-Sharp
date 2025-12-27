using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;

namespace GSharp.CodeGen;

// This class emits IL for 'if' statements.
public static class IfEmitter
{
    public static void Emit(
        ILGenerator il,
        IfStatement ifStmt,
        Dictionary<string, LocalBuilder> locals)
    {
        // Labels used to control where execution jumps.
        // elseLabel is used when the condition is false.
        // endLabel marks the point after the if/else finishes.
        var elseLabel = il.DefineLabel();
        var endLabel = il.DefineLabel();

        // Emit the condition expression.
        // This leaves exactly one object on the stack.
        ExpressionEmitter.EmitToStack(il, ifStmt.Condition, locals);

        // Ask the runtime if this value should be treated as "true".
        // This is where dynamic behavior lives.
        il.Emit(OpCodes.Call,
            typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsTrue))!);

        // If the result is false, jump to the else block.
        il.Emit(OpCodes.Brfalse, elseLabel);

        // If we get here, the condition was true.
        // Emit all statements inside the 'then' block.
        foreach (var stmt in ifStmt.ThenBody)
            StatementEmitter.Emit(il, stmt, locals);

        // Skip the else block after executing the then block.
        il.Emit(OpCodes.Br, endLabel);

        // Execution jumps here when the condition is false.
        il.MarkLabel(elseLabel);

        // Emit the else block, if it exists.
        if (ifStmt.ElseBody != null)
        {
            foreach (var stmt in ifStmt.ElseBody)
                StatementEmitter.Emit(il, stmt, locals);
        }

        // Execution continues here after the if/else finishes.
        il.MarkLabel(endLabel);
    }
}
