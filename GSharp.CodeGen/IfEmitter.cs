using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;

namespace GSharp.CodeGen;

// Emits IL for 'if/then/else' expressions.
//
// if is an expression — both branches must leave exactly one value on the stack.
// When there is no else branch, the if evaluates to null when the condition is false.
//
// IL structure:
//
//   <emit condition>
//   Call RuntimeHelpers.IsTrue
//   Brfalse elseLabel
//     <emit then body — discard all but last>
//     <emit last of then body>          ; this is the expression value
//   Br endLabel
//   elseLabel:
//     <emit else body — discard all but last>
//     <emit last of else body>          ; this is the expression value
//     (if no else: Ldnull)
//   endLabel:
//   ; one value on the stack
public static class IfEmitter
{
    public static void EmitToStack(ILGenerator il, IfExpression ifExpr, EmitContext ctx)
    {
        var elseLabel = il.DefineLabel();
        var endLabel  = il.DefineLabel();

        ExpressionEmitter.EmitToStack(il, ifExpr.Condition, ctx);

        il.Emit(OpCodes.Call,
            typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsTrue))!);

        il.Emit(OpCodes.Brfalse, elseLabel);

        // Then branch — leave last expression's value on the stack.
        EmitBody(il, ifExpr.ThenBody, ctx);

        il.Emit(OpCodes.Br, endLabel);

        il.MarkLabel(elseLabel);

        if (ifExpr.ElseBody is { Count: > 0 })
            EmitBody(il, ifExpr.ElseBody, ctx);
        else
            il.Emit(OpCodes.Ldnull);

        il.MarkLabel(endLabel);
    }

    // Emits a body (list of expressions), discarding all values except the last.
    // The last expression's value is left on the stack.
    private static void EmitBody(ILGenerator il, List<Expression> body, EmitContext ctx)
    {
        for (var i = 0; i < body.Count - 1; i++)
        {
            ExpressionEmitter.EmitToStack(il, body[i], ctx);
            il.Emit(OpCodes.Pop);
        }

        if (body.Count > 0)
            ExpressionEmitter.EmitToStack(il, body[^1], ctx);
        else
            il.Emit(OpCodes.Ldnull);
    }
}
