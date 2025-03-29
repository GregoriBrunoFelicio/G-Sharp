using System.Text.RegularExpressions;
using G.Sharp.Compiler.AST;
using Type = G.Sharp.Compiler.AST.Type;

namespace G.Sharp.Compiler.Parsers.Shared;

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
        };

        return reserved.Contains(word);
    }

    public static bool IsTypeCompatible(Type expected, VariableValue actual) =>
        expected == actual.Type;
}