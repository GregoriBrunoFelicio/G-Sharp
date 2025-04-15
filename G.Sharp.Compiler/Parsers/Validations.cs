using System.Text.RegularExpressions;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.Parsers;

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

    // I really need to change it, don't make sense this check
    public static bool IsTypeCompatible(GType expected, GType actual) =>
        expected.Kind switch
        {
            GPrimitiveType.Number => NumericTypes.Contains(actual.Kind) && expected.IsArray == actual.IsArray,
            _ => expected.Kind == actual.Kind && expected.IsArray == actual.IsArray
        };
}