using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

public static class LetEmitter
{
    public static void Emit(ILGenerator il, LetStatement statement, Dictionary<string, LocalBuilder> locals)
    {
        ExpressionEmitter.EmitToStack(il, statement.Expression, locals);

        var local = il.DeclareLocal(typeof(object));
        il.Emit(OpCodes.Stloc, local);

        locals[statement.VariableName] = local;
    }
}