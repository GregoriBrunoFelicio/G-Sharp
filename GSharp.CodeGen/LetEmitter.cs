using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// Emits IL for 'let' bindings.
//
// Example:
//   let x = a + 1
//
// The emitted IL:
//   <emit value>        ; leaves one object on the stack
//   Stloc slot_N        ; pops the object and stores it in local slot N
//   Ldnull              ; let-binding evaluates to null (unit-like)
//
// After this, 'x' is registered in ctx.Locals so that any later
// BindingExpression("x") can emit Ldloc(slot_N) to retrieve the value.
public static class LetEmitter
{
    public static void Emit(ILGenerator il, LetExpression expr, EmitContext ctx)
    {
        ExpressionEmitter.EmitToStack(il, expr.Value, ctx);

        // Allocate a new local slot. Everything in G# is dynamically typed,
        // so the slot type is always object.
        var local = il.DeclareLocal(typeof(object));
        il.Emit(OpCodes.Stloc, local);

        ctx.Locals[expr.BindingName] = local;

        // let-binding returns null — it is a side-effecting expression.
        il.Emit(OpCodes.Ldnull);
    }
}
