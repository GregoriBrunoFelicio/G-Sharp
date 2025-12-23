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

            // empilha o elemento (object)
            ExpressionEmitter.EmitLiteralToStack(il, array[i]);

            // armazena no array
            il.Emit(OpCodes.Stelem_Ref);
        }
    }
}