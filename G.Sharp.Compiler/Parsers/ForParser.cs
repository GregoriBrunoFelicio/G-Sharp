using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Parsers;

public class ForParser(Parser parser)
{
    public ForStatement Parse()
    {
        var loopVar = parser.Identifier().Value;

        parser.Consume(TokenType.In);

        var iterable = new ExpressionParser(parser).Parse();

        parser.Consume(TokenType.LeftBrace);

        var body = new List<Statement>();
        
        while (!parser.Check(TokenType.RightBrace))
        {
            var stmt = parser.ParseNextStatement();
            body.Add(stmt);
        }

        parser.Consume(TokenType.RightBrace);

        return new ForStatement(loopVar, iterable, body);
    }
}