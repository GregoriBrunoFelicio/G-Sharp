using System.Diagnostics;
using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class WhileParser(Parser parser)
{
    public WhileStatement Parse()
    {
        parser.Consume(TokenType.While);
        
        var condition = new ExpressionParser(parser).Parse();

        parser.Consume(TokenType.LeftBrace);

        var body = new List<Statement>();

        while (!parser.Check(TokenType.RightBrace))
        {
            var statement = parser.ParseNextStatement();
            body.Add(statement);
        }

        parser.Consume(TokenType.RightBrace);

        return new WhileStatement(condition, body);
    }
}