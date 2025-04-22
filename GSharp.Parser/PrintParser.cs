using GSharp.AST;

namespace GSharp.Parser;

public class PrintParser(Parser parser)
{
    public PrintStatement Parse()
    {
        var expression = new ExpressionParser(parser).Parse();
        parser.Semicolon();
        return new PrintStatement(expression);
    }
}