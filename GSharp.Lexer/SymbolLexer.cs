using static GSharp.Lexer.Helpers.SymbolTokenMap;

namespace GSharp.Lexer;

public static class SymbolLexer
{
    public static Token Read(Lexer lexer)
    {
        var current = lexer.Current;
        var next = lexer.Next();

        switch (current)
        {
            case '>' when next == '=':
                lexer.Advance();
                lexer.Advance();
                return new Token(TokenType.GreaterThanOrEqual, ">=");
            case '<' when next == '=':
                lexer.Advance();
                lexer.Advance();
                return new Token(TokenType.LessThanOrEqual, "<=");
            case '=' when next == '=':
                lexer.Advance();
                lexer.Advance();
                return new Token(TokenType.EqualEqual, "==");
            case '!' when next == '=':
                lexer.Advance();
                lexer.Advance();
                return new Token(TokenType.NotEqual, "!=");
        }

        if (Symbols.TryGetValue(current, out var tokenType))
        {
            lexer.Advance();
            return new Token(tokenType, current.ToString());
        }

        throw new Exception($"Unexpected symbol: '{current}'");
    }
}