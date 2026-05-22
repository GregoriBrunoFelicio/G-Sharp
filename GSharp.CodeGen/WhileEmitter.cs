using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;

namespace GSharp.CodeGen;

// Emits IL for 'while condition do' expressions.
//
// NOTE: while requires mutable state to be useful and is deprecated.
// It will be removed once recursion covers its use cases.
//
// while evaluates to null — it is a side-effecting expression.
//
// IL structure:
//
//   startLabel:
//     <emit condition>
//     Call RuntimeHelpers.IsTrue
//     Brfalse endLabel
//     <emit body — values discarded>
//     Br startLabel
//   endLabel:
//   Ldnull
public static class WhileEmitter
{
    public static void Emit(ILGenerator il, WhileExpression expr, EmitContext ctx)
    {
        var startLabel = il.DefineLabel(); // top of the loop — condition is re-evaluated here
        var endLabel   = il.DefineLabel(); // after the loop — execution continues here

        il.MarkLabel(startLabel);

        ExpressionEmitter.EmitToStack(il, expr.Condition, ctx);

        // Convert the condition value to a bool via the runtime helper.
        // This handles G#'s dynamic typing — the condition could be any object.
        il.Emit(OpCodes.Call,
            typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsTrue))!);

        il.Emit(OpCodes.Brfalse, endLabel);

        foreach (var bodyExpr in expr.Body)
        {
            ExpressionEmitter.EmitToStack(il, bodyExpr, ctx);
            il.Emit(OpCodes.Pop);
        }

        il.Emit(OpCodes.Br, startLabel);

        il.MarkLabel(endLabel);

        // while evaluates to null.
        il.Emit(OpCodes.Ldnull);
    }
}
