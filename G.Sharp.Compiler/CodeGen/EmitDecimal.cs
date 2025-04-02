using System.Reflection.Emit;

namespace G.Sharp.Compiler.CodeGen;

public static class EmitDecimal
{
    public static LocalBuilder Emit(ILGenerator il, decimal value)
    {
        var local = il.DeclareLocal(typeof(decimal));

        var ctor = typeof(decimal).GetConstructor([typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte)]);

        var bits = decimal.GetBits(value);
        var lo = bits[0];
        var mid = bits[1];
        var hi = bits[2];
        var sign = (bits[3] & 0x80000000) != 0;
        var scale = (byte)((bits[3] >> 16) & 0x7F);

        il.Emit(OpCodes.Ldc_I4, lo);
        il.Emit(OpCodes.Ldc_I4, mid);
        il.Emit(OpCodes.Ldc_I4, hi);
        il.Emit(sign ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ldc_I4_S, scale);
        il.Emit(OpCodes.Newobj, ctor);
        il.Emit(OpCodes.Stloc, local);

        return local;
    }
}