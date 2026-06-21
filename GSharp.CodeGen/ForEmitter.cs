using System.Reflection.Emit;
using GSharp.AST;
using GSharp.TypeChecker;

namespace GSharp.CodeGen;

public static class ForEmitter
{
    public static void Emit(ILGenerator il, ForExpression forExpression, EmitContext context)
    {
        ExpressionEmitter.EmitToStack(il, forExpression.Iterable, context);
        il.Emit(OpCodes.Castclass, typeof(object[]));
        var arrayLocal = il.DeclareLocal(typeof(object[]));
        il.Emit(OpCodes.Stloc, arrayLocal);

        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Conv_I4);
        il.Emit(OpCodes.Newarr, typeof(object));
        var resultLocal = il.DeclareLocal(typeof(object[]));
        il.Emit(OpCodes.Stloc, resultLocal);

        var indexLocal = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc, indexLocal);

        var loopStart = il.DefineLabel();
        var loopEnd   = il.DefineLabel();

        il.MarkLabel(loopStart);

        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Conv_I4);
        il.Emit(OpCodes.Bge, loopEnd);

        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldelem_Ref);

        var elementClrType = ResolveElementClrType(forExpression, context);
        if (elementClrType != typeof(object))
            il.Emit(OpCodes.Unbox_Any, elementClrType);

        var loopVar = il.DeclareLocal(elementClrType);
        il.Emit(OpCodes.Stloc, loopVar);
        context.Locals[forExpression.BindingName] = loopVar;

        for (var i = 0; i < forExpression.Body.Count - 1; i++)
        {
            ExpressionEmitter.EmitToStack(il, forExpression.Body[i], context);
            il.Emit(OpCodes.Pop);
        }

        il.Emit(OpCodes.Ldloc, resultLocal);
        il.Emit(OpCodes.Ldloc, indexLocal);
        ExpressionEmitter.EmitToStack(il, forExpression.Body[^1], context);
        il.Emit(OpCodes.Stelem_Ref);

        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc, indexLocal);
        il.Emit(OpCodes.Br, loopStart);

        il.MarkLabel(loopEnd);

        il.Emit(OpCodes.Ldloc, resultLocal);
    }

    private static Type ResolveElementClrType(ForExpression forExpression, EmitContext context)
    {
        if (!context.TypeMap.TryGetValue(forExpression.Iterable, out var gsType))
            return typeof(object);

        return gsType is ArrayType arrayType
            ? FunctionEmitter.GsTypeToClr(arrayType.ElementType)
            : typeof(object);
    }
}
