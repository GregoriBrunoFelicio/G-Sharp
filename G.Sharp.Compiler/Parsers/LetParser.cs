using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using static G.Sharp.Compiler.Parsers.Validations;

namespace G.Sharp.Compiler.Parsers;

public class LetParser(Parser parser)
{
    // TODO: Add support to expressions
    public LetStatement Parse()
    {
        var variableName = GetVariableName();

        var varType = GetVariableType();

        var value = GetVariableValue(varType);

        if (!IsTypeCompatible(varType, value))
        {
            throw new Exception($"Type mismatch: expected {varType}, but got {value.Type}");
        }

        parser.Consume(TokenType.Semicolon);

        parser.VariablesDeclared.Add(variableName, varType);

        return new LetStatement(variableName, value);
    }
    
    private string GetVariableName()
    {
        var name = parser.Consume(TokenType.Identifier).Value;
        ValidateVariableName(name);
        parser.Consume(TokenType.Colon);

        return name;
    }
    
    private VariableValue GetVariableValue(GType expectedType)
    {
        var valueParser = new ValueParser(parser);
        parser.Consume(TokenType.Equals);
        return valueParser.Parse(expectedType);
    }

    private GType GetVariableType()
    {
        var type = GetPrimitiveType();
        var isArray = IsArrayType();
        return new GType(type, isArray);
    }

    private bool IsArrayType()
    {
        if (!parser.Match(TokenType.LeftBracket)) return false;
        parser.Consume(TokenType.RightBracket);
        return true;
    }

    private GPrimitiveType GetPrimitiveType()
    {
        if (parser.Match(TokenType.Number))
            return GPrimitiveType.Number;

        if (parser.Match(TokenType.String))
            return GPrimitiveType.String;

        if (parser.Match(TokenType.Boolean))
            return GPrimitiveType.Boolean;

        throw new Exception("Expected a valid primitive type.");
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
}