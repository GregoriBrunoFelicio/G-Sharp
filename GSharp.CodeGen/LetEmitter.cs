using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// Emits IL for 'let' bindings.
//
// Example:
//   let x = a + 1
//
// The emitted IL:
//   <emit expression>   ; leaves one object on the stack
//   Stloc slot_N        ; pops the object and stores it in local slot N
//
// After this, 'x' is registered in ctx.Locals so that any later
// BindingExpression("x") can emit Ldloc(slot_N) to retrieve the value.
public static class LetEmitter
{
    public static void Emit(ILGenerator il, LetStatement statement, EmitContext ctx)
    {
        // Emit the right-hand side expression.
        // This must leave exactly one boxed object on the stack.
        ExpressionEmitter.EmitToStack(il, statement.Expression, ctx);

        // Allocate a new local slot in the current method frame.
        // Everything in G# is dynamically typed, so the slot type is always object.
        var local = il.DeclareLocal(typeof(object));

        // Pop the value from the stack and store it in the newly allocated slot.
        il.Emit(OpCodes.Stloc, local);

        // Register the binding name so subsequent statements can find it.
        ctx.Locals[statement.BindingName] = local;
    }
}
