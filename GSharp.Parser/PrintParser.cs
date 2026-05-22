using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class PrintParser(Parser parser)
{
    public PrintExpression Parse()
    {
        parser.Consume(TokenType.Println);
        var value = new ExpressionParser(parser).Parse();
        return new PrintExpression(value);
    }
}
