namespace G.Sharp.Compiler.Lexer;

public static class NumberLexer
{
    private static readonly HashSet<char> Suffixes = ['f', 'F', 'd', 'D', 'm', 'M'];

    public static Token Read(Lexer lexer)
    {
        var start = lexer.Position;

        lexer.AdvanceWhile(char.IsDigit);

        if (!lexer.IsAtEnd() && lexer.Current == '.')
        {
            lexer.Advance();
            lexer.AdvanceWhile(char.IsDigit);
        }

        if (!lexer.IsAtEnd() && Suffixes.Contains(lexer.Current))
            lexer.Advance();

        var number = lexer.Code[start..lexer.Position];
        
        return new Token(TokenType.NumberLiteral, number);
    }
}