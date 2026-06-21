namespace GSharp.Lexer.Helpers;

public static class SymbolTokenMap
{
    public static readonly Dictionary<char, TokenType> Symbols = new()
    {
        // '=' must be gated here so SymbolLexer is called for '==' and '=>'.
        // A bare '=' still produces Equals, which the parser will reject.
        { '=', TokenType.Equals },

        // Parentheses
        { '(', TokenType.LeftParen },
        { ')', TokenType.RightParen },

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
        { '*', TokenType.Multiply },
        { '/', TokenType.Divide },

        // Module access
        { '.', TokenType.Dot },
    };
}