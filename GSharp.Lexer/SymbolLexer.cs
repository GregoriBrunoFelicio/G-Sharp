using static GSharp.Lexer.Helpers.SymbolTokenMap;

namespace GSharp.Lexer;

public static class SymbolLexer
{
    public static Token Read(Lexer lexer)
    {
        var line    = lexer.Line;
        var col     = lexer.Column;
        var current = lexer.Current;
        var next    = lexer.Next();

        switch (current)
        {
            case '-' when next == '>':
                lexer.Advance(); lexer.Advance();
                return new Token(TokenType.ThinArrow, "->", line, col);
            case '=' when next == '>':
                lexer.Advance(); lexer.Advance();
                return new Token(TokenType.Arrow, "=>", line, col);
            case '>' when next == '=':
                lexer.Advance(); lexer.Advance();
                return new Token(TokenType.GreaterThanOrEqual, ">=", line, col);
            case '<' when next == '=':
                lexer.Advance(); lexer.Advance();
                return new Token(TokenType.LessThanOrEqual, "<=", line, col);
            case '=' when next == '=':
                lexer.Advance(); lexer.Advance();
                return new Token(TokenType.EqualEqual, "==", line, col);
            case '!' when next == '=':
                lexer.Advance(); lexer.Advance();
                return new Token(TokenType.NotEqual, "!=", line, col);
        }

        if (Symbols.TryGetValue(current, out var tokenType))
        {
            lexer.Advance();
            return new Token(tokenType, current.ToString(), line, col);
        }

        throw new Exception($"{line}: unexpected '{current}'");
    }
}
