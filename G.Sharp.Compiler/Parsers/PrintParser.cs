using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Parsers;

public class PrintParser(Parser parser)
{
    public Statement Parse()
    {
        var name = parser.Identifier().Value;
        parser.Semicolon();
        return new PrintStatement(name);
    }
}