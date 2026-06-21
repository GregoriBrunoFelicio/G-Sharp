using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;

namespace GSharp.CodeGen;

public static class IfEmitter
{
    private static readonly MethodInfo IsTrueMethod =
        typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsTrue))!;

    public static void EmitToStack(ILGenerator il, IfExpression ifExpression, EmitContext context)
    {
        var elseLabel = il.DefineLabel();
        var endLabel  = il.DefineLabel();

        var condType = ExpressionEmitter.Emit(il, ifExpression.Condition, context);
        if (condType == typeof(bool))
            il.Emit(OpCodes.Brfalse, elseLabel);
        else
        {
            il.Emit(OpCodes.Call, IsTrueMethod);
            il.Emit(OpCodes.Brfalse, elseLabel);
        }

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
        if (body.Count == 0) { il.Emit(OpCodes.Ldnull); return; }

        foreach (var expr in body.SkipLast(1))
        {
            ExpressionEmitter.EmitToStack(il, expr, context);
            il.Emit(OpCodes.Pop);
        }

        ExpressionEmitter.EmitToStack(il, body[^1], context);
    }
}
