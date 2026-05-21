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

        List<Statement> body;

        if (parser.Match(TokenType.Arrow))
        {
            // Inline form: accepts any single statement after =>
            //   soma(a b) => a + b            (expression → implicit return)
            //   greet()   => println "hello"  (statement → void-like, returns null)
            // Using ParseNextStatement instead of ExpressionParser so that statement
            // keywords like println are valid inline bodies.
            body = [parser.ParseNextStatement()];
        }
        else
        {
            // Block form: body is indented under the declaration
            parser.Match(TokenType.Newline);
            parser.Consume(TokenType.BlockOpen);
            body = ParseBody();
            parser.Consume(TokenType.BlockClose);
        }

        parser.VariablesDeclared.Add(name);
        return new FunctionDeclaration(name, parameters, body);
    }

    private List<Statement> ParseBody()
    {
        var statements = new List<Statement>();
        while (!parser.Check(TokenType.BlockClose))
        {
            if (parser.Match(TokenType.Newline)) continue;
            statements.Add(parser.ParseNextStatement());
        }
        return statements;
    }
}
