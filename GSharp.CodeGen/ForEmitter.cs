using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// This class is responsible for emitting IL for 'for' loops.
public static class ForEmitter
{
    public static void Emit(
        ILGenerator il,
        ForStatement statement,
        Dictionary<string, LocalBuilder> variables)
    {
        // 1) Evaluate the iterable expression
        // This leaves exactly ONE object on the stack.
        // At runtime, we expect this object to be an object[].
        ExpressionEmitter.EmitToStack(il, statement.Iterable, variables);

        // Cast the value to object[].
        // If the value is not an array, this will throw at runtime.
        // That is a language error, not a codegen concern.
        il.Emit(OpCodes.Castclass, typeof(object[]));

        // Store the array reference in a local variable.
        var arrayLocal = il.DeclareLocal(typeof(object[]));
        il.Emit(OpCodes.Stloc, arrayLocal);

        // int index = 0;
        var indexLocal = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc, indexLocal);

        // Labels that define the loop structure.
        var loopStart = il.DefineLabel();
        var loopEnd = il.DefineLabel();

        // Jump target for the beginning of each iteration.
        il.MarkLabel(loopStart);

        // if (index >= array.Length)
        //     break;
        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldlen);     // array.Length (native unsigned)
        il.Emit(OpCodes.Conv_I4);   // convert length to int32
        il.Emit(OpCodes.Bge, loopEnd);

        // element = array[index]
        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldelem_Ref);

        // Store the current element in a local variable.
        // The loop variable is always an object,
        // since the language is dynamically typed.
        var loopVar = il.DeclareLocal(typeof(object));
        il.Emit(OpCodes.Stloc, loopVar);

        // Bind the loop variable name to the local.
        // From this point on, any reference to the loop variable
        // inside the body will resolve to this local.
        variables[statement.Variable] = loopVar;

        // Emit each statement inside the loop body.
        foreach (var s in statement.Body)
            StatementEmitter.Emit(il, s, variables);

        // index++
        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc, indexLocal);

        // Jump back to the beginning of the loop.
        il.Emit(OpCodes.Br, loopStart);

        // Execution continues here after the loop finishes.
        il.MarkLabel(loopEnd);
    }
}
