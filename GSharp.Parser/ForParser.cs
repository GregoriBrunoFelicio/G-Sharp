using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class ForParser(Parser parser)
{
    public ForExpression Parse()
    {
        parser.Consume(TokenType.For);

        var bindingName = parser.Identifier().Value;

        parser.Consume(TokenType.In);

        var iterable = new ExpressionParser(parser).Parse();

        parser.Consume(TokenType.Do);
        parser.Match(TokenType.Newline);
        parser.Consume(TokenType.BlockOpen);

        var body = new List<Expression>();

        while (!parser.Check(TokenType.BlockClose))
        {
            if (parser.Match(TokenType.Newline))
                continue;

            body.Add(parser.ParseNext());
        }

        parser.Consume(TokenType.BlockClose);

        return new ForExpression(bindingName, iterable, body);
    }
}
