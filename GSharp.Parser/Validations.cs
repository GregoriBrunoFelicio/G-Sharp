using System.Text.RegularExpressions;
using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public static partial class Validations
{
    [GeneratedRegex("^[a-zA-Z_][a-zA-Z0-9_]*$")]
    private static partial Regex ValidVariableRegex();

    public static bool IsValidVariableName(string name) =>
        ValidVariableRegex().IsMatch(name);

    public static bool IsReserved(string word)
    {
        var reserved = new HashSet<string>
        {
            "let",
            "if",
            "else",
            "while",
            "for",
            "return",
            "true",
            "false",
            "null",
            "function",
            "print",
            "printf",
            "println",
            "string",
            "number",
            "boolean",
            "bool",
            "in",
            "int",
            "float",
            "char",
            "void",
        };

        return reserved.Contains(word);
    }

    private static readonly HashSet<GPrimitiveType> NumericTypes =
    [
        GPrimitiveType.Int,
        GPrimitiveType.Float,
        GPrimitiveType.Double,
        GPrimitiveType.Decimal
    ];

    public static bool IsTypeCompatible(GType expected, GType actual)
    {
        if (expected.IsArray != actual.IsArray)
            return false;

        var sameKind = expected.Kind == actual.Kind;

        var isNumberSuperType =
            expected.Kind == GPrimitiveType.Number &&
            NumericTypes.Contains(actual.Kind);

        return sameKind || isNumberSuperType;
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