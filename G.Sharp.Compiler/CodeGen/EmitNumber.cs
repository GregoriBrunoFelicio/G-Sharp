using System.Reflection.Emit;

namespace G.Sharp.Compiler.CodeGen;

public static class EmitNumber
{
    public static LocalBuilder EmitInt(ILGenerator il, int value)
    {
        il.Emit(OpCodes.Ldc_I4, value);
        var local = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Stloc, local);
        return local;
    }

    public static LocalBuilder EmitFloat(ILGenerator il, float value)
    {
        il.Emit(OpCodes.Ldc_R4, value);
        var local = il.DeclareLocal(typeof(float));
        il.Emit(OpCodes.Stloc, local);
        return local;
    }

    public static LocalBuilder EmitDouble(ILGenerator il, double value)
    {
        il.Emit(OpCodes.Ldc_R8, value);
        var local = il.DeclareLocal(typeof(double));
        il.Emit(OpCodes.Stloc, local);
        return local;
    }
}