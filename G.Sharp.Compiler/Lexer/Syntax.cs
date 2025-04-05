namespace G.Sharp.Compiler.Lexer;

public static class Syntax
{
    public static readonly Dictionary<char, TokenType> SymbolTokenMap = new()
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