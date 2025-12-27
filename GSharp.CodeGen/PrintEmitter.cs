using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// This class is responsible for emitting IL for 'print' / 'println' statements.

public static class PrintEmitter
{
    public static void Emit(
        ILGenerator il,
        PrintStatement statement,
        Dictionary<string, LocalBuilder> locals)
    {
        // Emit the expression to be printed.
        // This leaves exactly one object on the stack.
        ExpressionEmitter.EmitToStack(il, statement.Expression, locals);

        // Resolve Console.WriteLine(object).
        // Since everything in the language is an object,
        // this overload fits perfectly.
        var method = typeof(Console)
            .GetMethod("WriteLine", [typeof(object)])!;

        // Call Console.WriteLine with the value on the stack.
        il.Emit(OpCodes.Call, method);
    }
}
