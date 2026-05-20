using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class IfParser(Parser parser)
{
    public IfStatement Parse()
    {
        parser.Consume(TokenType.If);

        var condition = new ExpressionParser(parser).Parse();

        parser.Consume(TokenType.Then);

        var thenBody = parser.Check(TokenType.Newline) ? BlockBody() : InlineBody();

        var elseBody = new List<Statement>();

        if (parser.Match(TokenType.Else))
        {
            var elseStatements = parser.Check(TokenType.Newline) ? BlockBody() : InlineBody();
            elseBody.AddRange(elseStatements);
        }

        return new IfStatement(condition, thenBody, elseBody);
    }

    private List<Statement> BlockBody()
    {
        parser.Match(TokenType.Newline);
        parser.Consume(TokenType.BlockOpen);

        var statements = new List<Statement>();

        while (!parser.Check(TokenType.BlockClose))
        {
            if (parser.Match(TokenType.Newline))
                continue;

            statements.Add(parser.ParseNextStatement());
        }

        parser.Consume(TokenType.BlockClose);

        return statements;
    }

    private List<Statement> InlineBody() => [parser.ParseNextStatement()];
}
