using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// This class is responsible for emitting IL for variable declarations.
public static class LetEmitter
{
    public static void Emit(
        ILGenerator il,
        LetStatement statement,
        Dictionary<string, LocalBuilder> locals)
    {
        // Emit the expression assigned to the variable.
        // This leaves exactly one object on the stack.
        ExpressionEmitter.EmitToStack(il, statement.Expression, locals);

        // Declare a new local variable to hold the value.
        // Since the language is dynamic, the local type is always object.
        var local = il.DeclareLocal(typeof(object));

        // Store the value from the stack into the local variable.
        il.Emit(OpCodes.Stloc, local);

        // Register the variable name so it can be referenced later.
        locals[statement.VariableName] = local;
    }
}
