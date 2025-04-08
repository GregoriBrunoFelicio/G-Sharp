
namespace G.Sharp.Compiler.Lexer;

public static class IdentifierLexer
{
    public static Token Read(Lexer lexer)
    {
        var start = lexer.Position;

        lexer.AdvanceWhile(char.IsLetterOrDigit);

        var value = lexer.Code[start..lexer.Position];

        var tokenType = KeywordTokenMap.GetValueOrDefault(value, TokenType.Identifier);

        return new Token(tokenType, value);
    }

    private static readonly Dictionary<string, TokenType> KeywordTokenMap = new()
    {
        ["let"] = TokenType.Let,
        ["number"] = TokenType.Number,
        ["string"] = TokenType.String,
        ["println"] = TokenType.Println,
        ["bool"] = TokenType.Boolean,
        ["true"] = TokenType.BooleanTrueLiteral,
        ["false"] = TokenType.BooleanFalseLiteral,
        ["if"] = TokenType.If,
        ["else"] = TokenType.Else,
        ["for"] = TokenType.For,
        ["in"] = TokenType.In,
        ["while"] = TokenType.While,
    };
}