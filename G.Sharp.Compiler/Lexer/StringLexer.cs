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

        var word = lexer.Code[start..lexer.Position];

        lexer.Advance();

        return new Token(TokenType.StringLiteral, word);
    }
}