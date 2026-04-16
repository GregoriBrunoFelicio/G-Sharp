using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class PrintParser(Parser parser)
{
    public PrintStatement Parse()
    {
        parser.Consume(TokenType.Println);
        var expression = new ExpressionParser(parser).Parse();
        return new PrintStatement(expression);
    }
}