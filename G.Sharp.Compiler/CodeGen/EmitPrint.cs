using System.Reflection;
using System.Reflection.Emit;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.CodeGen;

public static class EmitPrint
{
    public static void Emit(ILGenerator il, PrintStatement statement, Dictionary<string, LocalBuilder> locals)
    {
        if (!locals.TryGetValue(statement.VariableName, out var variable))
            throw new InvalidOperationException($"Variable '{statement.VariableName}' is not defined.");

        il.Emit(OpCodes.Ldloc, variable);

        var method = typeof(Console).GetMethod(
            "WriteLine",
            BindingFlags.Public | BindingFlags.Static,
            null,
            [variable.LocalType],
            null);

        if (method == null)
            throw new MissingMethodException(
                $"No suitable Console.WriteLine method found for type '{variable.LocalType.Name}'.");

        il.Emit(OpCodes.Call, method);
    }
}