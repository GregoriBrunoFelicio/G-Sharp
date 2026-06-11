using GSharp.AST;
using GSharp.Lexer;
using static GSharp.Parser.Validations;

namespace GSharp.Parser;

internal class ArrayParser(Parser parser)
{
    public LiteralExpression Parse()
    {
        var elements = new List<object>();
        Type? elementType = null;

        while (!parser.Check(TokenType.RightBracket))
        {
            var expr = new ExpressionParser(parser).Parse();

            if (expr is not LiteralExpression lit)
                throw new Exception("Only literal expressions are supported in arrays for now.");

            var value = lit.Value;
            elementType ??= value?.GetType();

            if (value?.GetType() != elementType)
                throw new Exception("Array literals must contain elements of the same type.");

            elements.Add(lit.Value);
        }

        parser.Consume(TokenType.RightBracket);
        return new LiteralExpression(elements.ToArray());
    }
}
