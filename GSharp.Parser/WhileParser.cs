using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class WhileParser(Parser parser)
{
    public WhileStatement Parse()
    {
        parser.Consume(TokenType.While);
        
        var condition = new ExpressionParser(parser).Parse();

        parser.Consume(TokenType.Do);
        parser.Match(TokenType.Newline);
        parser.Consume(TokenType.Indent);

        var body = new List<Statement>();

        while (!parser.Check(TokenType.Dedent))
        {
            if (parser.Match(TokenType.Newline))
                continue;

            var statement = parser.ParseNextStatement();
            body.Add(statement);
        }

        parser.Consume(TokenType.Dedent);

        return new WhileStatement(condition, body);
    }
}