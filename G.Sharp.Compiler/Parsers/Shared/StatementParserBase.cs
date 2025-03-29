using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Parsers.Shared;

public abstract class StatementParserBase(ParserHelper parserHelper)
{
    protected Token Consume(TokenType type) => parserHelper.Consume(type);
    protected bool Match(TokenType type) => parserHelper.Match(type);
    protected bool Check(TokenType type) => parserHelper.Check(type);
    protected Token Advance() => parserHelper.Advance();
    protected bool IsAtEnd() => parserHelper.IsAtEnd();
}