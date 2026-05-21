using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// Emits IL for 'println' statements.
//
// Example:
//   println x + 1
//
// The emitted IL:
//   <emit expression>            ; leaves one object on the stack
//   Call Console.WriteLine(object)  ; pops the object and prints it
//
// We use the Console.WriteLine(object) overload specifically because
// everything in G# is already boxed as object. This overload calls
// ToString() internally, so any value type or reference type prints correctly.
public static class PrintEmitter
{
    public static void Emit(ILGenerator il, PrintStatement statement, EmitContext ctx)
    {
        // Emit the expression to be printed.
        // This leaves exactly one object on the stack.
        ExpressionEmitter.EmitToStack(il, statement.Expression, ctx);

        // Resolve Console.WriteLine(object).
        // Since everything in the language is an object,
        // this overload fits perfectly.
        var method = typeof(Console)
            .GetMethod("WriteLine", [typeof(object)])!;

        // Call Console.WriteLine with the value on the stack.
        il.Emit(OpCodes.Call, method);
    }
}
