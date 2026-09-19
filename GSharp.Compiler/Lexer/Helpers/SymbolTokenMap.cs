namespace GSharp.Compiler.Lexer.Helpers;

public static class SymbolTokenMap
{
    public static readonly Dictionary<char, TokenType> Symbols = new()
    {
        { '=', TokenType.Equals },

        // Parentheses
        { '(', TokenType.LeftParen },
        { ')', TokenType.RightParen },

        // Brackets
        { '[', TokenType.LeftBracket },
        { ']', TokenType.RightBracket },
        { '{', TokenType.LeftBrace },
        { '}', TokenType.RightBrace },
        { ':', TokenType.Colon },

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
        { '.', TokenType.Dot }
    };
}