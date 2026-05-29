namespace GSharp.Lexer;

public static class StringLexer
{
    public static Token Read(Lexer lexer)
    {
        var line = lexer.Line;
        var col  = lexer.Column;

        lexer.Advance(); // skip opening "

        var start = lexer.Position;
        lexer.AdvanceWhile(c => c != '"');

        if (lexer.IsAtEnd())
            throw new Exception($"{line}: unterminated string literal");

        var word = lexer.Code[start..lexer.Position];
        lexer.Advance(); // skip closing "

        return new Token(TokenType.StringLiteral, word, line, col);
    }
}