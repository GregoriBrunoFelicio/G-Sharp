namespace G.Sharp.Compiler.Lexer;

public static class StringLexer
{
    public static Token Read(Lexer lexer)
    {
        lexer.Advance();

        var start = lexer.Position;

        while (!lexer.IsAtEnd() && lexer.Current != '"')
            lexer.Advance();

        if (lexer.IsAtEnd())
            throw new Exception("Unterminated string literal. Expected closing '\"'.");

        var end = lexer.Position;
        var value = lexer.Code[start..end];

        lexer.Advance();

        return new Token(TokenType.StringLiteral, value);
    }
}