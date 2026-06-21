using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;


public static class PrintEmitter
{
    public static void Emit(ILGenerator il, PrintExpression printExpression, EmitContext context)
    {
        ExpressionEmitter.EmitToStack(il, printExpression.Value, context);
        var method = typeof(Console)
            .GetMethod("WriteLine", [typeof(object)])!;
        il.Emit(OpCodes.Call, method);
        il.Emit(OpCodes.Ldnull);
    }
}