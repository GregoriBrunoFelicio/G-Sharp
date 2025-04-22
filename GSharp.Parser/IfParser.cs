using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class IfParser(Parser parser)
{
    public IfStatement Parse()
    {
        parser.Consume(TokenType.If);

        var condition = new ExpressionParser(parser).Parse(); 

        var thenBody = ContentBody();

        var elseBody = new List<Statement>();

        if (!parser.Match(TokenType.Else)) return new IfStatement(condition, thenBody, elseBody);
        var statements = ContentBody();
        elseBody.AddRange(statements);

        return new IfStatement(condition, thenBody, elseBody);
    }

    private List<Statement> ContentBody()
    {
        parser.Consume(TokenType.LeftBrace);

        var statements = new List<Statement>();

        while (!parser.Check(TokenType.RightBrace))
        {
            var statement = parser.ParseNextStatement();
            statements.Add(statement);
        }

        parser.Consume(TokenType.RightBrace);

        return statements;
    }
}