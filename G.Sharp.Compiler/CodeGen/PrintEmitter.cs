using System.Reflection;
using System.Reflection.Emit;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.CodeGen;

public static class PrintEmitter
{
    public static void Emit(ILGenerator il, PrintStatement statement, Dictionary<string, LocalBuilder> locals)
    {
        ExpressionEmitter.EmitToStack(il, statement.Expression, locals);

        var type = statement.Expression switch
        {
            LiteralExpression lit => lit.Value.Type.GetClrType(),
            VariableExpression v when locals.TryGetValue(v.Name, out var local) => local.LocalType,
            _ => throw new NotSupportedException($"Cannot infer type of expression: {statement.Expression}")
        };

        var method = typeof(Console).GetMethods()
            .FirstOrDefault(m =>
                m.Name == "WriteLine" &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == type);

        if (method == null)
            throw new MissingMethodException(
                $"No suitable Console.WriteLine method found for type '{type.Name}'.");

        il.Emit(OpCodes.Call, method);
    }
}