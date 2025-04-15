using System.Reflection.Emit;

namespace G.Sharp.Compiler.CodeGen;

public static class BooleanEmitter
{
    public static void Emit(ILGenerator il, bool value)
    {
        il.Emit(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        var local = il.DeclareLocal(typeof(bool));
        il.Emit(OpCodes.Stloc, local);
    }
}