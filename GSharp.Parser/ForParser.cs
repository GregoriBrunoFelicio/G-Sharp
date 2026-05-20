using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class ForParser(Parser parser)
{
    public ForStatement Parse()
    {
        parser.Consume(TokenType.For);
        
        var loopVar = parser.Identifier().Value;

        parser.Consume(TokenType.In);

        var iterable = new ExpressionParser(parser).Parse();

        parser.Consume(TokenType.Do);
        parser.Match(TokenType.Newline);
        parser.Consume(TokenType.BlockOpen);

        var body = new List<Statement>();

        while (!parser.Check(TokenType.BlockClose))
        {
            if (parser.Match(TokenType.Newline))
                continue;

            var statement = parser.ParseNextStatement();
            body.Add(statement);
        }

        parser.Consume(TokenType.BlockClose);

        return new ForStatement(loopVar, iterable, body);
    }
}