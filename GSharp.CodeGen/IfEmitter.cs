using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;

namespace GSharp.CodeGen;

// Emits IL for 'if/then/else' statements.
//
// IL does not have a native "if" instruction. Conditional execution is
// implemented using two primitives:
//   - Brfalse: jump to a label if the top of the stack is false/zero/null
//   - Br: unconditional jump
//
// The structure emitted looks like this:
//
//   <emit condition>        ; leaves one object on stack
//   Call RuntimeHelpers.IsTrue  ; converts object → bool, leaves bool on stack
//   Brfalse elseLabel       ; jump to else if condition is false
//   <emit then body>
//   Br endLabel             ; skip the else block
//   elseLabel:
//   <emit else body>        ; only if an else branch exists
//   endLabel:
//   <continue>
public static class IfEmitter
{
    public static void Emit(ILGenerator il, IfStatement ifStmt, EmitContext ctx)
    {
        // Labels used to control where execution jumps.
        // elseLabel: where to jump when the condition is false.
        // endLabel: where both branches converge after execution.
        var elseLabel = il.DefineLabel();
        var endLabel = il.DefineLabel();

        // Emit the condition expression.
        // This leaves exactly one object on the stack.
        ExpressionEmitter.EmitToStack(il, ifStmt.Condition, ctx);

        // Ask the runtime if this value should be treated as "true".
        // IsTrue handles dynamic typing: it unboxes and checks the bool.
        // Result: a bool is left on the stack (1 = true, 0 = false).
        il.Emit(OpCodes.Call,
            typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsTrue))!);

        // If the result is false (0), jump past the then-body to elseLabel.
        il.Emit(OpCodes.Brfalse, elseLabel);

        // Condition was true — emit all statements in the then-body.
        foreach (var stmt in ifStmt.ThenBody)
            StatementEmitter.Emit(il, stmt, ctx);

        // Jump past the else-body so we don't fall through into it.
        il.Emit(OpCodes.Br, endLabel);

        // Execution lands here when the condition was false.
        il.MarkLabel(elseLabel);

        // Emit the else body, if one exists.
        if (ifStmt.ElseBody != null)
            foreach (var stmt in ifStmt.ElseBody)
                StatementEmitter.Emit(il, stmt, ctx);

        // Both branches merge here. Execution continues normally.
        il.MarkLabel(endLabel);
    }
}
