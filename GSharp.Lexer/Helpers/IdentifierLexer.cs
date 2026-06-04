
namespace GSharp.Lexer.Helpers;

public static class IdentifierLexer
{
    public static Token Read(Lexer lexer)
    {
        var line = lexer.Line;
        var col  = lexer.Column;
        var start = lexer.Position;

        lexer.AdvanceWhile(char.IsLetterOrDigit);

        var value = lexer.Code[start..lexer.Position];
        var tokenType = KeywordTokenMap.GetValueOrDefault(value, TokenType.Identifier);

        return new Token(tokenType, value, line, col);
    }

    private static readonly Dictionary<string, TokenType> KeywordTokenMap = new()
    {
        // Declarations
        ["let"] = TokenType.Let,
        ["number"] = TokenType.Number,
        ["string"] = TokenType.String,
        ["bool"] = TokenType.Boolean,

        // Booleans
        ["true"] = TokenType.BooleanTrueLiteral,
        ["false"] = TokenType.BooleanFalseLiteral,

        // Conditionals
        ["if"] = TokenType.If,
        ["else"] = TokenType.Else,

        // Loops
        ["for"] = TokenType.For,
        ["in"] = TokenType.In,
        ["do"] = TokenType.Do,
        ["then"] = TokenType.Then,

        // IO
        ["println"] = TokenType.Println,

        // Logical operators
        ["and"] = TokenType.And,
        ["or"] = TokenType.Or,
        ["not"] = TokenType.Not,
        
        // Functions
        ["function"] = TokenType.Function,

        // Imports
        ["import"] = TokenType.Import,
    };
}