using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;

namespace GSharp.CodeGen;

// Emits IL for 'while condition do' loops.
//
// NOTE: while loops require mutable state to be useful (the condition must
// eventually become false). Since G# is moving toward pure functional style,
// while is deprecated and will be removed once recursion covers its use cases.
//
// The emitted IL structure:
//
//   startLabel:
//     <emit condition>           ; leaves one object on stack
//     Call RuntimeHelpers.IsTrue ; converts object → bool
//     Brfalse endLabel           ; exit if false
//     <emit body>
//     Br startLabel              ; loop back
//   endLabel:
//   <continue>
public static class WhileEmitter
{
    public static void Emit(ILGenerator il, WhileStatement whileStmt, EmitContext ctx)
    {
        // startLabel: the top of the loop where the condition is re-evaluated
        // endLabel: where execution continues after the loop exits
        var startLabel = il.DefineLabel();
        var endLabel   = il.DefineLabel();

        // All iterations begin here.
        il.MarkLabel(startLabel);

        // Emit the loop condition.
        // This leaves exactly one object on the stack.
        ExpressionEmitter.EmitToStack(il, whileStmt.Condition, ctx);

        // Convert the condition value to a bool via the runtime helper.
        // This handles G#'s dynamic typing — the condition could be any object.
        il.Emit(OpCodes.Call,
            typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsTrue))!);

        // If the condition is false, exit the loop.
        il.Emit(OpCodes.Brfalse, endLabel);

        // Condition was true — emit all statements inside the loop body.
        foreach (var stmt in whileStmt.Body)
            StatementEmitter.Emit(il, stmt, ctx);

        // Jump back to the top and re-evaluate the condition.
        il.Emit(OpCodes.Br, startLabel);

        // Execution continues here once the condition becomes false.
        il.MarkLabel(endLabel);
    }
}
