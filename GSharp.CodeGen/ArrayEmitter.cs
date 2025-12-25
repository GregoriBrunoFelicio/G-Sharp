using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

public static class ArrayEmitter
{
    public static void EmitToStack(ILGenerator il, object[] array)
    {
        // tamanho do array
        il.Emit(OpCodes.Ldc_I4, array.Length);

        // cria object[]
        il.Emit(OpCodes.Newarr, typeof(object));

        for (var i = 0; i < array.Length; i++)
        {
            il.Emit(OpCodes.Dup);        // duplica referência do array
            il.Emit(OpCodes.Ldc_I4, i);  // índice

            // empilha o valor diretamente
            EmitObject(il, array[i]);

            // array[i] = value
            il.Emit(OpCodes.Stelem_Ref);
        }
    }

    private static void EmitObject(ILGenerator il, object value)
    {
        switch (value)
        {
            case int i:
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Box, typeof(int));
                break;

            case double d:
                il.Emit(OpCodes.Ldc_R8, d);
                il.Emit(OpCodes.Box, typeof(double));
                break;

            case bool b:
                il.Emit(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Box, typeof(bool));
                break;

            case string s:
                il.Emit(OpCodes.Ldstr, s);
                break;

            default:
                throw new NotSupportedException(
                    $"Unsupported array literal element: {value?.GetType().Name}");
        }
    }
}