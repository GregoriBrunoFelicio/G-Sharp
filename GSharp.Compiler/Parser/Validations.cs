using System.Globalization;
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
        "if", "else", "for", "return",
        "true", "false", "null",
        "function", "print", "printf", "println",
        "in", "main", "import",
    ];

    public static bool IsReserved(string word) => ReservedKeywords.Contains(word);

    public static bool IsLiteralToken(TokenType t) =>
        t is TokenType.NumberLiteral
          or TokenType.StringLiteral
          or TokenType.BooleanTrueLiteral
          or TokenType.BooleanFalseLiteral;
    
    internal static object ParseNumber(string text)
    {
        var ic = CultureInfo.InvariantCulture;
        if (text.EndsWith('f')) return float.Parse(text[..^1], ic);
        if (text.EndsWith('d')) return double.Parse(text[..^1], ic);
        if (text.EndsWith('m')) return decimal.Parse(text[..^1], ic);
        if (text.Contains('.'))  return double.Parse(text, ic);
        return int.Parse(text, ic);
    }

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