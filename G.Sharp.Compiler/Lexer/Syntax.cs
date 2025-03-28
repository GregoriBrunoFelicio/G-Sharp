namespace G.Sharp.Compiler.Lexer;

public static class Syntax
{
    public static readonly Dictionary<char, TokenType> SymbolTokenMap = new()
    {
        { ':', TokenType.Colon },
        { '=', TokenType.Equals },
        { ';', TokenType.Semicolon }
    };

    public static readonly Dictionary<string, TokenType> KeywordTokenMap = new()
    {
        ["let"] = TokenType.Let,
        ["number"] = TokenType.Number,
        ["string"] = TokenType.String,
        ["println"] = TokenType.Println,
        ["bool"] = TokenType.Boolean,
        ["true"] = TokenType.BooleanTrueLiteral,
        ["false"] = TokenType.BooleanFalseLiteral,
    };
}