using System.Reflection.Emit;

namespace GSharp.CodeGen;
// This class is responsible for emitting IL for array literals.
public static class ArrayEmitter
{
    // Emits IL that creates an object[] and pushes it onto the stack.
    public static void EmitToStack(ILGenerator il, object[] array)
    {
        // Push the array length onto the stack.
        // This will be used by the 'newarr' instruction.
        il.Emit(OpCodes.Ldc_I4, array.Length);

        // Create a new object[] with the given length.
        // After this instruction, the stack contains:
        //   object[] (the newly created array)
        il.Emit(OpCodes.Newarr, typeof(object));

        // Fill the array one element at a time.
        for (var i = 0; i < array.Length; i++)
        {
            // Duplicate the array reference.
            //
            // We need this because:
            // - Stelem_Ref consumes the array reference
            // - But we still need it for the next iteration
            il.Emit(OpCodes.Dup);

            // Push the index where the value will be stored.
            il.Emit(OpCodes.Ldc_I4, i);

            // Push the element value onto the stack.
            // This method is responsible for boxing value types.
            EmitObject(il, array[i]);

            // Store the value into array[index].
            //
            // Stack before:
            //   array, index, value
            //
            // Stack after:
            //   array
            il.Emit(OpCodes.Stelem_Ref);
        }

        // At the end of the loop, the stack contains:
        //   object[] (fully initialized)
        //
        // This satisfies the expression contract.
    }

    // Emits IL to push a single object value onto the stack.
    private static void EmitObject(ILGenerator il, object value)
    {
        switch (value)
        {
            case int i:
                // Push int32 literal
                il.Emit(OpCodes.Ldc_I4, i);

                // Box it to object
                il.Emit(OpCodes.Box, typeof(int));
                break;

            case double d:
                // Push double literal
                il.Emit(OpCodes.Ldc_R8, d);

                // Box it to object
                il.Emit(OpCodes.Box, typeof(double));
                break;

            case bool b:
                // Push 1 or 0 depending on the boolean value
                il.Emit(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);

                // Box it to object
                il.Emit(OpCodes.Box, typeof(bool));
                break;

            case string s:
                // Strings are reference types.
                // No boxing needed.
                il.Emit(OpCodes.Ldstr, s);
                break;

            default:
                // If we reach this point, it means the language
                // produced an array literal with a value we don't support yet.
                //
                // This is a language limitation, not an IL problem.
                throw new NotSupportedException(
                    $"Unsupported array literal element: {value?.GetType().Name}");
        }
    }
}
