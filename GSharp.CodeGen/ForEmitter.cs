using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// Emits IL for 'for item in collection do' expressions.
//
// for is a functional map — it transforms each element and returns a new array.
// The generated IL is equivalent to this C# pattern:
//
//   object[] array  = (object[]) <iterable>;
//   object[] result = new object[array.Length];
//   int index = 0;
//   while (index < array.Length)
//   {
//       object item    = array[index];
//       <body expressions except last — values discarded>
//       result[index]  = <last body expression>;
//       index++;
//   }
//   return result;
//
// The loop binding (e.g. 'item') is registered in ctx.Locals so that
// expressions inside the body can reference it like any other binding.
public static class ForEmitter
{
    public static void Emit(ILGenerator il, ForExpression expr, EmitContext ctx)
    {
        // ============================
        // 1. Evaluate and store the iterable
        // ============================

        ExpressionEmitter.EmitToStack(il, expr.Iterable, ctx);
        il.Emit(OpCodes.Castclass, typeof(object[]));
        var arrayLocal = il.DeclareLocal(typeof(object[]));
        il.Emit(OpCodes.Stloc, arrayLocal);

        // ============================
        // 2. Allocate result array of the same length
        // ============================

        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Conv_I4);
        il.Emit(OpCodes.Newarr, typeof(object));
        var resultLocal = il.DeclareLocal(typeof(object[]));
        il.Emit(OpCodes.Stloc, resultLocal);

        // ============================
        // 3. Initialize the index
        // ============================

        var indexLocal = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc, indexLocal);

        // ============================
        // 4. Loop structure
        // ============================

        var loopStart = il.DefineLabel();
        var loopEnd   = il.DefineLabel();

        il.MarkLabel(loopStart);

        // ============================
        // 5. Condition: index < array.Length
        // ============================

        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Conv_I4);
        il.Emit(OpCodes.Bge, loopEnd);

        // ============================
        // 6. Load current element: item = array[index]
        // ============================

        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldelem_Ref);
        var loopVar = il.DeclareLocal(typeof(object));
        il.Emit(OpCodes.Stloc, loopVar);
        ctx.Locals[expr.BindingName] = loopVar;

        // ============================
        // 7. Emit body — discard all but the last expression
        //    Last expression becomes the element in result[index]
        // ============================

        for (var i = 0; i < expr.Body.Count - 1; i++)
        {
            ExpressionEmitter.EmitToStack(il, expr.Body[i], ctx);
            il.Emit(OpCodes.Pop);
        }

        // Store last expression into result[index].
        // Stelem_Ref expects stack: [array, index, value]
        il.Emit(OpCodes.Ldloc, resultLocal);
        il.Emit(OpCodes.Ldloc, indexLocal);
        ExpressionEmitter.EmitToStack(il, expr.Body[^1], ctx);
        il.Emit(OpCodes.Stelem_Ref);

        // ============================
        // 8. Increment index and loop back
        // ============================

        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc, indexLocal);
        il.Emit(OpCodes.Br, loopStart);

        il.MarkLabel(loopEnd);

        // Return the result array — satisfies the one-object-on-stack contract.
        il.Emit(OpCodes.Ldloc, resultLocal);
    }
}
