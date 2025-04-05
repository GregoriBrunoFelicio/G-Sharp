using System.Reflection.Emit;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.CodeGen;

public static class ArrayEmitter
{
    public static LocalBuilder Emit(ILGenerator il, ArrayValue array)
    {
        var elementType = GetSystemType(array.ElementType);
        var local = il.DeclareLocal(elementType.MakeArrayType());

        il.Emit(OpCodes.Ldc_I4, array.Elements.Count);
        il.Emit(OpCodes.Newarr, elementType);

        for (var i = 0; i < array.Elements.Count; i++)
        {
            il.Emit(OpCodes.Dup); // Thinking about it :/
            il.Emit(OpCodes.Ldc_I4, i);

            EmitElement(il, array.Elements[i]);

            var storeOpCode = GetStelemOpCode(elementType);
            
            il.Emit(storeOpCode);
        }

        il.Emit(OpCodes.Stloc, local);
        return local;
    }

    private static Type GetSystemType(GType type)
    {
        return type.Kind switch
        {
            GPrimitiveType.Number => typeof(int), // TODO: Add another number types like float, double, decimal
            GPrimitiveType.String => typeof(string),
            GPrimitiveType.Boolean => typeof(bool),
            _ => throw new NotSupportedException($"Unknown GType {type.Kind}")
        };
    }

    private static void EmitElement(ILGenerator il, VariableValue value)
    {
        switch (value)
        {
            case IntValue intVal:
                il.Emit(OpCodes.Ldc_I4, intVal.Value);
                break;
            case FloatValue floatVal:
                il.Emit(OpCodes.Ldc_R4, floatVal.Value);
                break;
            case DoubleValue doubleVal:
                il.Emit(OpCodes.Ldc_R8, doubleVal.Value);
                break;
            case DecimalValue decVal:
                DecimalEmitter.Emit(il, decVal.Value);
                break;
            case StringValue strVal:
                il.Emit(OpCodes.Ldstr, strVal.Value);
                break;
            case BooleanValue boolVal:
                il.Emit(boolVal.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                break;
            default:
                throw new NotSupportedException($"Unsupported array element type: {value.GetType().Name}");
        }
    }

    private static OpCode GetStelemOpCode(Type type)
    {
        if (type == typeof(int)) return OpCodes.Stelem_I4;
        if (type == typeof(float)) return OpCodes.Stelem_R4;
        if (type == typeof(double)) return OpCodes.Stelem_R8;
        if (type == typeof(string)) return OpCodes.Stelem_Ref;
        if (type == typeof(bool)) return OpCodes.Stelem_I1;
        if (type == typeof(decimal))
            throw new NotSupportedException("Arrays of decimal are not supported"); // TODO: Implement this

        throw new NotSupportedException($"Unsupported array element type: {type}");
    }
}