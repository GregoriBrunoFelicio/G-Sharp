namespace GSharp.AST;

public static class ExpressionExtensions
{
    public static VariableValue GetLiteralValue(this Expression expression)
    {
        if (expression is LiteralExpression literal)
            return literal.Value;

        throw new Exception("Expected a literal expression.");
    }
}