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

        var varType = GetVariableType();

        parser.Equals();
        var expression = new ExpressionParser(parser).Parse();

        var value = expression.GetLiteralValue();

        if (!IsTypeCompatible(varType, value.Type))
        {
            throw new Exception($"Type mismatch: expected {varType}, but got {value.Type}");
        }

        parser.Semicolon();

        parser.VariablesDeclared.Add(variableName, varType);

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
        if (parser.VariablesDeclared.ContainsKey(variableName))
            throw new Exception($"Variable {variableName} already declared.");

        if (!IsValidVariableName(variableName))
            throw new Exception($"Invalid variable name: {variableName}");

        if (IsReserved(variableName))
            throw new Exception($"'{variableName}' is a reserved keyword.");
    }

    private GType GetVariableType()
    {
        parser.Colon();
        var baseType = GetConcreteType();
        var isArray = IsArrayType();
        return isArray ? new GArrayType(baseType) : baseType;
    }

    private bool IsArrayType()
    {
        if (!parser.Match(TokenType.LeftBracket)) return false;
        parser.Consume(TokenType.RightBracket);
        return true;
    }

    private GType GetConcreteType()
    {
        if (parser.Match(TokenType.Number))
            return new GNumberType();

        if (parser.Match(TokenType.String))
            return new GStringType();

        if (parser.Match(TokenType.Boolean))
            return new GBooleanType();

        throw new Exception($"Unknown type: {parser.Current().Type}");
    }
}