using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;

namespace GSharp.CodeGen;

// This class is responsible for emitting IL for 'while' loops.
public static class WhileEmitter
{
    public static void Emit(
        ILGenerator il,
        WhileStatement whileStmt,
        Dictionary<string, LocalBuilder> locals)
    {
        // Labels that control the loop flow.
        // startLabel marks the beginning of the loop.
        // endLabel marks where execution continues after the loop ends.
        var startLabel = il.DefineLabel();
        var endLabel = il.DefineLabel();

        // This is where each loop iteration starts.
        il.MarkLabel(startLabel);

        // Emit the loop condition.
        // This leaves exactly one object on the stack.
        ExpressionEmitter.EmitToStack(il, whileStmt.Condition, locals);

        // Ask the runtime if the value should be treated as true.
        il.Emit(OpCodes.Call,
            typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsTrue))!);

        // If the condition is false, exit the loop.
        il.Emit(OpCodes.Brfalse, endLabel);

        // If we get here, the condition was true.
        // Emit all statements inside the loop body.
        foreach (var stmt in whileStmt.Body)
            StatementEmitter.Emit(il, stmt, locals);

        // Jump back to the beginning and evaluate the condition again.
        il.Emit(OpCodes.Br, startLabel);

        // Execution continues here once the loop finishes.
        il.MarkLabel(endLabel);
    }
}
