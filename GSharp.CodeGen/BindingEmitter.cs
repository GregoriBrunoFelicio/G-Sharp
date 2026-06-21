using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

public static class BindingEmitter
{
    public static void Emit(ILGenerator il, BindingExpression binding, EmitContext context)
    {
        var clrType = ExpressionEmitter.Emit(il, binding.Value, context);
        var local = il.DeclareLocal(clrType);
        il.Emit(OpCodes.Stloc, local);
        context.Locals[binding.BindingName] = local;
        il.Emit(OpCodes.Ldnull);
    }
}
