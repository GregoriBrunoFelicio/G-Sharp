using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// Emits IL for 'for item in collection do' loops.
//
// G# for-loops iterate over object[] arrays (the only collection type right now).
// The generated IL is equivalent to this C# pattern:
//
//   object[] array = (object[]) <iterable expression>;
//   int index = 0;
//   while (index < array.Length)
//   {
//       object item = array[index];
//       <body statements>
//       index++;
//   }
//
// The loop binding (e.g. 'item') is registered in ctx.Locals so that
// statements inside the body can reference it like any other binding.
public static class ForEmitter
{
    public static void Emit(ILGenerator il, ForStatement statement, EmitContext ctx)
    {
        // ============================
        // 1. Evaluate the iterable
        // ============================

        // Emit the expression that produces the collection.
        // Leaves one object on the stack.
        ExpressionEmitter.EmitToStack(il, statement.Iterable, ctx);

        // Cast the object to object[].
        // If the value is not an array, this throws InvalidCastException at runtime.
        // That is a language-level type error, not a codegen concern.
        il.Emit(OpCodes.Castclass, typeof(object[]));

        // Store the array reference in a local so we can read it each iteration.
        var arrayLocal = il.DeclareLocal(typeof(object[]));
        il.Emit(OpCodes.Stloc, arrayLocal);

        // ============================
        // 2. Initialize the index
        // ============================

        // int index = 0;
        var indexLocal = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc, indexLocal);

        // ============================
        // 3. Loop structure
        // ============================

        // Labels that define the loop structure.
        var loopStart = il.DefineLabel(); // top of the loop — condition check
        var loopEnd   = il.DefineLabel(); // after the loop — break target

        // This is where execution returns at the end of each iteration.
        il.MarkLabel(loopStart);

        // ============================
        // 4. Condition: index < array.Length
        // ============================

        il.Emit(OpCodes.Ldloc, indexLocal);   // push index
        il.Emit(OpCodes.Ldloc, arrayLocal);   // push array reference
        il.Emit(OpCodes.Ldlen);               // push array.Length (as native uint)
        il.Emit(OpCodes.Conv_I4);             // convert to int32 for comparison

        // If index >= length, exit the loop.
        il.Emit(OpCodes.Bge, loopEnd);

        // ============================
        // 5. Load the current element: item = array[index]
        // ============================

        il.Emit(OpCodes.Ldloc, arrayLocal);   // push array reference
        il.Emit(OpCodes.Ldloc, indexLocal);   // push current index
        il.Emit(OpCodes.Ldelem_Ref);          // push array[index] (an object reference)

        // Store the element in a new local slot.
        // This is the loop binding (e.g. 'item' in 'for item in nums do').
        var loopVar = il.DeclareLocal(typeof(object));
        il.Emit(OpCodes.Stloc, loopVar);

        // Register the loop binding name so body statements can access it.
        // From this point on, any BindingExpression with this name resolves
        // to Ldloc(loopVar).
        ctx.Locals[statement.BindingName] = loopVar;

        // ============================
        // 6. Emit the loop body
        // ============================

        foreach (var s in statement.Body)
            StatementEmitter.Emit(il, s, ctx);

        // ============================
        // 7. Increment index and loop back
        // ============================

        // index++
        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc, indexLocal);

        // Jump back to the condition check.
        il.Emit(OpCodes.Br, loopStart);

        // Execution continues here after the loop finishes.
        il.MarkLabel(loopEnd);
    }
}
