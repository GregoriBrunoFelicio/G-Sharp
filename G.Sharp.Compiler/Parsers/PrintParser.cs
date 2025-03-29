using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Parsers;

public class PrintParser(Parser parser)
{
    public Statement Parse()
    {
        var name = parser.Consume(TokenType.Identifier).Value;
        parser.Consume(TokenType.Semicolon);
        return new PrintStatement(name);
    }
}