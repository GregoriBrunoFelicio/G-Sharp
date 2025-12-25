using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

public static class ForEmitter
{
    public static void Emit(
        ILGenerator il,
        ForStatement statement,
        Dictionary<string, LocalBuilder> variables)
    {
        // 1️⃣ Avalia o iterable (empilha object)
        ExpressionEmitter.EmitToStack(il, statement.Iterable, variables);

        // 2️⃣ Converte para object[]
        il.Emit(OpCodes.Castclass, typeof(object[]));

        var arrayLocal = il.DeclareLocal(typeof(object[]));
        il.Emit(OpCodes.Stloc, arrayLocal);

        // index = 0
        var indexLocal = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc, indexLocal);

        var loopStart = il.DefineLabel();
        var loopEnd = il.DefineLabel();

        il.MarkLabel(loopStart);

        // if index >= array.Length -> break
        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Conv_I4);
        il.Emit(OpCodes.Bge, loopEnd);

        // element = array[index]
        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldelem_Ref);

        var loopVar = il.DeclareLocal(typeof(object));
        il.Emit(OpCodes.Stloc, loopVar);

        variables[statement.Variable] = loopVar;

        // body
        foreach (var s in statement.Body)
            StatementEmitter.Emit(il, s, variables);

        // index++
        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc, indexLocal);

        il.Emit(OpCodes.Br, loopStart);
        il.MarkLabel(loopEnd);
    }
}