using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// Emits IL for 'for item in collection do' expressions.
//
// G# for-loops iterate over object[] arrays.
// The generated IL is equivalent to this C# pattern:
//
//   object[] array = (object[]) <iterable>;
//   int index = 0;
//   while (index < array.Length)
//   {
//       object item = array[index];
//       <body expressions — values discarded>
//       index++;
//   }
//
// for evaluates to null — it is a side-effecting expression.
// The loop binding (e.g. 'item') is registered in ctx.Locals so that
// expressions inside the body can reference it like any other binding.
public static class ForEmitter
{
    public static void Emit(ILGenerator il, ForExpression expr, EmitContext ctx)
    {
        // ============================
        // 1. Evaluate the iterable
        // ============================

        ExpressionEmitter.EmitToStack(il, expr.Iterable, ctx);

        // Cast the object to object[].
        // If the value is not an array, this throws InvalidCastException at runtime.
        // That is a language-level type error, not a codegen concern.
        il.Emit(OpCodes.Castclass, typeof(object[]));

        var arrayLocal = il.DeclareLocal(typeof(object[]));
        il.Emit(OpCodes.Stloc, arrayLocal);

        // ============================
        // 2. Initialize the index
        // ============================

        var indexLocal = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc, indexLocal);

        // ============================
        // 3. Loop structure
        // ============================

        var loopStart = il.DefineLabel(); // top of the loop — condition check
        var loopEnd   = il.DefineLabel(); // after the loop — break target

        il.MarkLabel(loopStart);

        // ============================
        // 4. Condition: index < array.Length
        // ============================

        il.Emit(OpCodes.Ldloc, indexLocal);   // push index
        il.Emit(OpCodes.Ldloc, arrayLocal);   // push array reference
        il.Emit(OpCodes.Ldlen);               // push array.Length (as native uint)
        il.Emit(OpCodes.Conv_I4);             // convert to int32 for comparison
        il.Emit(OpCodes.Bge, loopEnd);

        // ============================
        // 5. Load the current element: item = array[index]
        // ============================

        il.Emit(OpCodes.Ldloc, arrayLocal);   // push array reference
        il.Emit(OpCodes.Ldloc, indexLocal);   // push current index
        il.Emit(OpCodes.Ldelem_Ref);          // push array[index] (an object reference)

        // Store in a new local slot.
        // This is the loop binding (e.g. 'item' in 'for item in nums do').
        var loopVar = il.DeclareLocal(typeof(object));
        il.Emit(OpCodes.Stloc, loopVar);

        // Register the loop binding name so body expressions can access it.
        // From this point on, any BindingExpression with this name resolves
        // to Ldloc(loopVar).
        ctx.Locals[expr.BindingName] = loopVar;

        // ============================
        // 6. Emit the loop body — discard each expression's value
        // ============================

        foreach (var bodyExpr in expr.Body)
        {
            ExpressionEmitter.EmitToStack(il, bodyExpr, ctx);
            il.Emit(OpCodes.Pop);
        }

        // ============================
        // 7. Increment index and loop back
        // ============================

        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc, indexLocal);

        il.Emit(OpCodes.Br, loopStart);

        il.MarkLabel(loopEnd);

        // for evaluates to null.
        il.Emit(OpCodes.Ldnull);
    }
}
