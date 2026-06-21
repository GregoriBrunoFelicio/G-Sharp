using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;

namespace GSharp.CodeGen;

public static class TailCallEmitter
{
    public static void EmitTail(ILGenerator il, Expression expression, EmitContext context)
    {
        if (context.TailCall is { } tco)
        {
            if (TryEmitTailCall(il, expression, tco, context))
                return;

            if (expression is IfExpression ifExpression)
            {
                EmitTailIf(il, ifExpression, context);
                return;
            }
        }

        ExpressionEmitter.EmitToStack(il, expression, context);
    }

    private static bool TryEmitTailCall(
        ILGenerator il, Expression expression, TailCallInfo tco, EmitContext context)
    {
        if (expression is not CallExpression call) return false;
        if (call.Callee != tco.FunctionName) return false;
        if (call.Arguments.Count != tco.ParameterCount) return false;

        var paramTypes = context.FunctionParamTypes.GetValueOrDefault(tco.FunctionName);
        for (var i = 0; i < call.Arguments.Count; i++)
        {
            var expected = paramTypes is not null && i < paramTypes.Length ? paramTypes[i] : typeof(object);
            ExpressionEmitter.EmitArgAs(il, call.Arguments[i], expected, context);
        }

        for (var i = tco.ParameterCount - 1; i >= 0; i--)
            il.Emit(OpCodes.Starg_S, (byte)i);

        il.Emit(OpCodes.Br, tco.StartLabel);
        return true;
    }

    private static void EmitTailIf(ILGenerator il, IfExpression ifExpression, EmitContext context)
    {
        var elseLabel = il.DefineLabel();
        var endLabel  = il.DefineLabel();

        ExpressionEmitter.EmitToStack(il, ifExpression.Condition, context);
        il.Emit(OpCodes.Call, typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsTrue))!);
        il.Emit(OpCodes.Brfalse, elseLabel);

        EmitTailBody(il, ifExpression.ThenBody, context);
        il.Emit(OpCodes.Br, endLabel);

        il.MarkLabel(elseLabel);
        if (ifExpression.ElseBody is { Count: > 0 })
            EmitTailBody(il, ifExpression.ElseBody, context);
        else
            il.Emit(OpCodes.Ldnull);

        il.MarkLabel(endLabel);
    }

    private static void EmitTailBody(ILGenerator il, List<Expression> body, EmitContext context)
    {
        for (var i = 0; i < body.Count - 1; i++)
        {
            ExpressionEmitter.EmitToStack(il, body[i], context);
            il.Emit(OpCodes.Pop);
        }

        if (body.Count > 0)
            EmitTail(il, body[^1], context);
        else
            il.Emit(OpCodes.Ldnull);
    }
}
