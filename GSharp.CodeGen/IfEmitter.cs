using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;

namespace GSharp.CodeGen;

public static class IfEmitter
{
    public static void EmitToStack(ILGenerator il, IfExpression ifExpression, EmitContext context)
    {
        var elseLabel = il.DefineLabel();
        var endLabel  = il.DefineLabel();
        ExpressionEmitter.EmitToStack(il, ifExpression.Condition, context);
        il.Emit(OpCodes.Call,
            typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsTrue))!);
        il.Emit(OpCodes.Brfalse, elseLabel);
        EmitBody(il, ifExpression.ThenBody, context);
        il.Emit(OpCodes.Br, endLabel);
        il.MarkLabel(elseLabel);
        if (ifExpression.ElseBody is { Count: > 0 })
            EmitBody(il, ifExpression.ElseBody, context);
        else
            il.Emit(OpCodes.Ldnull);
        il.MarkLabel(endLabel);
    }

    private static void EmitBody(ILGenerator il, List<Expression> body, EmitContext context)
    {
        for (var i = 0; i < body.Count - 1; i++)
        {
            ExpressionEmitter.EmitToStack(il, body[i], context);
            il.Emit(OpCodes.Pop);
        }
        if (body.Count > 0)
            ExpressionEmitter.EmitToStack(il, body[^1], context);
        else
            il.Emit(OpCodes.Ldnull);
    }
}
