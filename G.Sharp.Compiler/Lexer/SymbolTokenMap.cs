namespace G.Sharp.Compiler.Lexer;

public static class SymbolTokenMap
{
    public static readonly Dictionary<char, TokenType> Symbols = new()
    {
        // Separators
        { ':', TokenType.Colon },
        { '=', TokenType.Equals },
        { ';', TokenType.Semicolon },

        // Brackets
        { '[', TokenType.LeftBracket },
        { ']', TokenType.RightBracket },
        { '{', TokenType.LeftBrace },
        { '}', TokenType.RightBrace },

        // Comparison
        { '>', TokenType.GreaterThan },
        { '<', TokenType.LessThan },
        { '!', TokenType.Not },

        // Arithmetic
        { '+', TokenType.Plus },
        { '-', TokenType.Minus },
        { '*', TokenType.Multiply }
    };
}