using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

public static class PrintEmitter
{
    public static void Emit(ILGenerator il, PrintStatement statement, Dictionary<string, LocalBuilder> locals)
    {
        ExpressionEmitter.EmitToStack(il, statement.Expression, locals);

        var method = typeof(Console)
            .GetMethod("WriteLine", new[] { typeof(object) })!;

        il.Emit(OpCodes.Call, method);
    }

    private static Type GetExpressionType(Expression expr, Dictionary<string, LocalBuilder> locals)
    {
        return expr switch
        {
            LiteralExpression lit => lit.Value.GetType(),
            VariableExpression v when locals.TryGetValue(v.Name, out var local) => local.LocalType,
            BinaryExpression b => GetExpressionType(b.Left, locals), 
            _ => throw new NotSupportedException($"Cannot infer type of expression: {expr}")
        };
    }
}