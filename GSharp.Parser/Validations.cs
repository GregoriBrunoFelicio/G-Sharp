using System.Text.RegularExpressions;
using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public static partial class Validations
{
    [GeneratedRegex("^[a-zA-Z_][a-zA-Z0-9_]*$")]
    private static partial Regex ValidBindingNameRegex();

    public static bool IsValidBindingName(string name) =>
        ValidBindingNameRegex().IsMatch(name);

    private static readonly HashSet<string> ReservedKeywords =
    [
        "let", "if", "else", "for", "return",
        "true", "false", "null",
        "function", "print", "printf", "println",
        "in", "main", "import",
    ];

    public static bool IsReserved(string word) => ReservedKeywords.Contains(word);
    
    public static readonly Dictionary<TokenType, int> OperatorPrecedence = new()
    {
        { TokenType.Or, 1 },
        { TokenType.And, 2 },
        { TokenType.EqualEqual, 3 },
        { TokenType.NotEqual, 3 },
        { TokenType.GreaterThan, 4 },
        { TokenType.GreaterThanOrEqual, 4 }, 
        { TokenType.LessThan, 4 },
        { TokenType.LessThanOrEqual, 4 },
        { TokenType.Plus, 5 },
        { TokenType.Minus, 5 },
        { TokenType.Multiply, 6 },
        { TokenType.Divide, 6 },
    };
}