using System.Reflection.Emit;

namespace GSharp.CodeGen;
public static class ArrayEmitter
{
    public static void EmitToStack(ILGenerator il, object[] array)
    {
        il.Emit(OpCodes.Ldc_I4, array.Length);
        il.Emit(OpCodes.Newarr, typeof(object));
        for (var i = 0; i < array.Length; i++)
        {
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4, i);
            EmitObject(il, array[i]);
            il.Emit(OpCodes.Stelem_Ref);
        }
    }

    private static void EmitObject(ILGenerator il, object value)
    {
        switch (value)
        {
            case int intValue:
                il.Emit(OpCodes.Ldc_I4, intValue);
                il.Emit(OpCodes.Box, typeof(int));
                break;
            case double doubleValue:
                il.Emit(OpCodes.Ldc_R8, doubleValue);
                il.Emit(OpCodes.Box, typeof(double));
                break;
            case bool boolValue:
                il.Emit(boolValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Box, typeof(bool));
                break;
            case string text:
                il.Emit(OpCodes.Ldstr, text);
                break;
            default:
                throw new NotSupportedException(
                    $"internal error: no emitter for array element type '{value?.GetType().Name ?? "null"}'");
        }
    }
}
