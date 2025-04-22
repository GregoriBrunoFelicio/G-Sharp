using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

public static class ArrayEmitter
{
    public static void EmitToStack(ILGenerator il, ArrayValue array)
    {
        var elementType = array.ElementType.GetClrType();

        il.Emit(OpCodes.Ldc_I4, array.Elements.Count); 
        il.Emit(OpCodes.Newarr, elementType); 

        for (var i = 0; i < array.Elements.Count; i++)
        {
            il.Emit(OpCodes.Dup); 
            il.Emit(OpCodes.Ldc_I4, i); 
            EmitElement(il, array.Elements[i]);
            il.Emit(GetStelemOpCode(elementType)); 
        }
    }

    private static void EmitElement(ILGenerator il, VariableValue value)
    {
        switch (value)
        {
            case IntValue i: il.Emit(OpCodes.Ldc_I4, i.Value); break;
            case FloatValue f: il.Emit(OpCodes.Ldc_R4, f.Value); break;
            case DoubleValue d: il.Emit(OpCodes.Ldc_R8, d.Value); break;
            case BooleanValue b: il.Emit(b.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0); break;
            case StringValue s: il.Emit(OpCodes.Ldstr, s.Value); break;
            default: throw new NotSupportedException($"Unsupported array element: {value.GetType().Name}");
        }
    }

    private static OpCode GetStelemOpCode(Type type)
    {
        if (type == typeof(int)) return OpCodes.Stelem_I4;
        if (type == typeof(float)) return OpCodes.Stelem_R4;
        if (type == typeof(double)) return OpCodes.Stelem_R8;
        if (type == typeof(string)) return OpCodes.Stelem_Ref;
        if (type == typeof(bool)) return OpCodes.Stelem_I1;
        throw new NotSupportedException($"Unsupported array element type: {type}");
    }
}