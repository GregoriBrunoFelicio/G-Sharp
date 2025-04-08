namespace G.Sharp.Compiler.Lexer;

public static class NumberLexer
{
    private static readonly HashSet<char> Suffixes = ['f', 'F', 'd', 'D', 'm', 'M'];

    public static Token Read(Lexer lexer)
    {
        var start = lexer.Position;

        lexer.AdvanceWhile(char.IsDigit);
        
        ReadDecimalIfExists(lexer);
        
        var number = lexer.Code[start..lexer.Position];
        return new Token(TokenType.NumberLiteral, number);
    }
    
    private static void ReadDecimalIfExists(Lexer lexer)
    {
        if (lexer.IsAtEnd() || lexer.Current != '.') return;
        lexer.Advance();
        lexer.AdvanceWhile(char.IsDigit);
    
        ReadNumberSuffix(lexer);
    }
    
    private static void ReadNumberSuffix(Lexer lexer)
    {
        if (lexer.IsAtEnd() || !Suffixes.Contains(lexer.Current)) return;
        lexer.Advance();
    }
}