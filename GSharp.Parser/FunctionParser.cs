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

        // The function name lives in the enclosing scope; its body gets a fresh scope.
        parser.DeclareBinding(name);

        var body = ParseScopedBody(parameters);

        // Span points at the function name so hovering it reports the full signature.
        return new FunctionDeclaration(name, parameters, body) { Line = nameToken.Line, Column = nameToken.Column };
    }

    // Parses the body inside a new lexical scope seeded with the parameter names, so a binding
    // can't reuse a parameter name (the codegen resolves parameters before locals, which would
    // otherwise make such a binding silently unreadable). The scope is always popped, even if the
    // body throws.
    private List<Expression> ParseScopedBody(List<string> parameters)
    {
        parser.EnterScope();
        try
        {
            foreach (var parameter in parameters)
                parser.DeclareBinding(parameter);

            if (parser.Match(TokenType.Arrow))
                return [parser.ParseNext()];

            parser.Match(TokenType.Newline);
            parser.Consume(TokenType.BlockOpen);
            var body = ParseBody();
            parser.Consume(TokenType.BlockClose);
            return body;
        }
        finally
        {
            parser.ExitScope();
        }
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
