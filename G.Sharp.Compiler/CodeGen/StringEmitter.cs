using System.Reflection.Emit;

namespace G.Sharp.Compiler.CodeGen;

public static class StringEmitter
{
    public static LocalBuilder Emit(ILGenerator il, string value)
    {
        il.Emit(OpCodes.Ldstr, value);
        var local = il.DeclareLocal(typeof(string));
        il.Emit(OpCodes.Stloc, local);
        return local;
    }
}