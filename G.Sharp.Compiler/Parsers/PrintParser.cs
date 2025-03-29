using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers.Shared;

namespace G.Sharp.Compiler.Parsers;

public class PrintParser(ParserHelper parserHelper) : StatementParserBase(parserHelper), IStatementParser
{
    public Statement Parse()
    {
        var name = Consume(TokenType.Identifier).Value;
        Consume(TokenType.Semicolon);
        return new PrintStatement(name);
    }
}