namespace G.Sharp.Compiler.Lexer;

public static class SymbolTokenMap
{
    public static readonly Dictionary<char, TokenType> Symbols = new()
    {
        { ':', TokenType.Colon },
        { '=', TokenType.Equals },
        { ';', TokenType.Semicolon },
        {'[', TokenType.LeftBracket },
        {']', TokenType.RightBracket},
        {'{', TokenType.LeftBrace },
        {'}', TokenType.RightBrace },
    };
}