using GSharp.AST;
using GSharp.Lexer;
using static GSharp.Parser.Validations;

namespace GSharp.Parser;

public class LetParser(Parser parser)
{
    public LetStatement Parse()
    {
        parser.Consume(TokenType.Let);
        var variableName = GetVariableName();

        parser.Equals();
        var expression = new ExpressionParser(parser).Parse();

        parser.VariablesDeclared.Add(variableName);

        return new LetStatement(variableName, expression);
    }

    private string GetVariableName()
    {
        var name = parser.Identifier().Value;
        ValidateVariableName(name);
        return name;
    }

    private void ValidateVariableName(string variableName)
    {
        if (parser.VariablesDeclared.Contains(variableName))
            throw new Exception($"Variable {variableName} already declared.");

        if (!IsValidVariableName(variableName))
            throw new Exception($"Invalid variable name: {variableName}");

        if (IsReserved(variableName))
            throw new Exception($"'{variableName}' is a reserved keyword.");
    }
}