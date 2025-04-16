using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Parsers;

public class PrintParser(Parser parser)
{
    public PrintStatement Parse()
    {
        var expression = new ExpressionParser(parser).Parse();
        parser.Semicolon();
        return new PrintStatement(expression);
    }
}