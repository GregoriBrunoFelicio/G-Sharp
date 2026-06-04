using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// Emits IL for 'let' bindings.
//
// Uses ExpressionEmitter.Emit (not EmitToStack) to get the value without unnecessary
// boxing. The declared local has the actual CLR type of the value:
//   let x = 10      → local is int32    (no heap allocation)
//   let d = 3.14d   → local is float64  (no heap allocation)
//   let s = "hello" → local is string   (reference, no boxing)
//   let r = f x     → local is object   (function return, fallback)
//
// When the binding is later loaded (BindingExpression), EmitBinding returns the local's
// actual type. EmitToStack then boxes it only if a boxed object is required by the consumer.
public static class LetEmitter
{
    public static void Emit(ILGenerator il, LetExpression expr, EmitContext ctx)
    {
        var clrType = ExpressionEmitter.Emit(il, expr.Value, ctx);

        var local = il.DeclareLocal(clrType);
        il.Emit(OpCodes.Stloc, local);

        ctx.Locals[expr.BindingName] = local;

        // let-binding returns null — it is a side-effecting expression.
        il.Emit(OpCodes.Ldnull);
    }
}
