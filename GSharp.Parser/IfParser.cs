using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class IfParser(Parser parser)
{
    public IfExpression Parse()
    {
        parser.Consume(TokenType.If);

        var condition = new ExpressionParser(parser).Parse();

        parser.Consume(TokenType.Then);

        var thenBody = parser.Check(TokenType.Newline) ? BlockBody() : InlineBody();

        List<Expression>? elseBody = null;

        if (parser.Match(TokenType.Else))
            elseBody = parser.Check(TokenType.Newline) ? BlockBody() : InlineBody();

        return new IfExpression(condition, thenBody, elseBody);
    }

    private List<Expression> BlockBody()
    {
        parser.Match(TokenType.Newline);
        parser.Consume(TokenType.BlockOpen);

        var expressions = new List<Expression>();

        while (!parser.Check(TokenType.BlockClose))
        {
            if (parser.Match(TokenType.Newline))
                continue;

            expressions.Add(parser.ParseNext());
        }

        parser.Consume(TokenType.BlockClose);

        return expressions;
    }

    private List<Expression> InlineBody() => [parser.ParseNext()];
}
