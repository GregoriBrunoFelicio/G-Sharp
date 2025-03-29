using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using static G.Sharp.Compiler.Parsers.Validations;
using Type = G.Sharp.Compiler.AST.Type;

namespace G.Sharp.Compiler.Parsers;

public class LetParser(Parser parser)
{
    private readonly HashSet<string> _variablesDeclared = [];

    public Statement Parse()
    {
        var variableName = parser.Consume(TokenType.Identifier).Value;

        ValidateVariableName(variableName);

        parser.Consume(TokenType.Colon);

        var varType = GetVariableType();

        parser.Consume(TokenType.Equals);

        var value = GetValue(varType);

        if (!IsTypeCompatible(varType, value))
        {
            throw new Exception($"Type mismatch: expected {varType}, but got {value.Type}");
        }

        parser.Consume(TokenType.Semicolon);

        _variablesDeclared.Add(variableName);

        return new LetStatement(variableName, value);
    }

    private Type GetVariableType()
    {
        if (parser.Match(TokenType.Number))
        {
            return Type.Number;
        }

        if (parser.Match(TokenType.String))
        {
            return Type.String;
        }

        if (parser.Match(TokenType.Boolean))
        {
            return Type.Boolean;
        }

        throw new Exception("Expected a type not found.");
    }

    private VariableValue GetValue(Type type) =>
        type switch
        {
            Type.Number => new NumberValue(int.Parse(parser.Consume(TokenType.NumberLiteral).Value)),
            Type.String => new StringValue(parser.Consume(TokenType.StringLiteral).Value),
            Type.Boolean => new BooleanValue(ConsumeBool()),
            _ => throw new Exception("Unsupported type")
        };

    private bool ConsumeBool()
    {
        if (parser.Match(TokenType.BooleanTrueLiteral)) return true;
        if (parser.Match(TokenType.BooleanFalseLiteral)) return false;
        throw new Exception("Expected boolean literal");
    }

    private void ValidateVariableName(string variableName)
    {
        if (_variablesDeclared.Contains(variableName))
            throw new Exception($"Variable {variableName} already declared.");

        if (!IsValidVariableName(variableName))
            throw new Exception($"Invalid variable name: {variableName}");

        if (IsReserved(variableName))
            throw new Exception($"'{variableName}' is a reserved keyword.");
    }
}