using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class FunctionParser(Parser parser)
{
    public FunctionDeclaration Parse()
    {
        var nameToken = parser.Identifier();
        var name      = nameToken.Value;

        var parameters = new List<string>();
        while (parser.Check(TokenType.Identifier))
            parameters.Add(parser.Identifier().Value);

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

        // Span points at the function name so hovering it reports the full signature.
        return new FunctionDeclaration(name, parameters, body) { Line = nameToken.Line, Column = nameToken.Column };
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
