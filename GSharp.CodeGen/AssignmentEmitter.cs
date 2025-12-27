using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// This class is responsible for emitting IL for variable assignment.
public static class AssignmentEmitter
{
    public static void Emit(
        ILGenerator il,
        AssignmentStatement statement,
        Dictionary<string, LocalBuilder> locals)
    {
        // Try to resolve the local variable.
        // If it doesn't exist, this is a language error,
        // not a code generation problem.
        if (!locals.TryGetValue(statement.VariableName, out var local))
            throw new Exception($"Variable '{statement.VariableName}' not found.");

        // Emit the expression.
        // This will leave exactly ONE object on the stack.
        ExpressionEmitter.EmitToStack(il, statement.Expression, locals);

        // Store the value into the local variable.
        //
        // Locals are always of type object,
        // since the language is dynamically typed.
        il.Emit(OpCodes.Stloc, local);
    }
}
