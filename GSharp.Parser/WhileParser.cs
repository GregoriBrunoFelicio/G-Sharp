using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class WhileParser(Parser parser)
{
    public WhileExpression Parse()
    {
        parser.Consume(TokenType.While);

        var condition = new ExpressionParser(parser).Parse();

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

        return new WhileExpression(condition, body);
    }
}
