using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// Emits IL for 'println' expressions.
//
// Example:
//   println x + 1
//
// The emitted IL:
//   <emit value>                     ; leaves one object on the stack
//   Call Console.WriteLine(object)   ; pops the object and prints it
//   Ldnull                           ; println evaluates to null (unit-like)
//
// We use the Console.WriteLine(object) overload because everything in G#
// is already boxed as object. This overload calls ToString() internally.
public static class PrintEmitter
{
    public static void Emit(ILGenerator il, PrintExpression printExpression, EmitContext context)
    {
        ExpressionEmitter.EmitToStack(il, printExpression.Value, context);

        // Console.WriteLine(object) — since everything in G# is boxed as object,
        // this overload fits perfectly and calls ToString() internally.
        var method = typeof(Console)
            .GetMethod("WriteLine", [typeof(object)])!;

        il.Emit(OpCodes.Call, method);

        // println returns null — it is a side-effecting expression.
        il.Emit(OpCodes.Ldnull);
    }
}
