namespace GSharp.Compiler.Lexer.Helpers;

public static class Constants
{
    public static readonly Dictionary<string, TokenType> KeywordTokenMap = new()
    {
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
        ["as"] = TokenType.As
    };

    public static readonly HashSet<char> NumberSuffixes = ['f', 'F', 'd', 'D', 'm', 'M'];
}
