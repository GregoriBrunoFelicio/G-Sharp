using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class FunctionParser(Parser parser)
{
    public FunctionDeclaration Parse()
    {
        var name = parser.Identifier().Value;
        parser.Consume(TokenType.LeftParen);

        var parameters = new List<string>();

        while (!parser.Check(TokenType.RightParen))
            parameters.Add(parser.Identifier().Value);

        parser.Consume(TokenType.RightParen);

        List<Expression> body;

        if (parser.Match(TokenType.Arrow))
        {
            body = [parser.ParseNext()];
        }
        else
        {
            parser.Match(TokenType.Newline);
            parser.Consume(TokenType.BlockOpen);
            body = ParseBody();
            parser.Consume(TokenType.BlockClose);
        }

        parser.DeclaredBindings.Add(name);
        return new FunctionDeclaration(name, parameters, body);
    }

    private List<Expression> ParseBody()
    {
        var expressions = new List<Expression>();
        while (!parser.Check(TokenType.BlockClose))
        {
            if (parser.Match(TokenType.Newline)) continue;
            expressions.Add(parser.ParseNext());
        }
        return expressions;
    }
}
