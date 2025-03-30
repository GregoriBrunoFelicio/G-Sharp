using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using static G.Sharp.Compiler.Parsers.Validations;

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

    private GType GetVariableType()
    {
        if (parser.Match(TokenType.Number))
        {
            return GType.Number;
        }

        if (parser.Match(TokenType.String))
        {
            return GType.String;
        }

        if (parser.Match(TokenType.Boolean))
        {
            return GType.Boolean;
        }

        throw new Exception("Expected a type not found.");
    }

    private VariableValue GetValue(GType type) =>
        type switch
        {
            GType.String => new StringValue(parser.Consume(TokenType.StringLiteral).Value),
            GType.Number => ParseNumber(),
            GType.Boolean => ParseBool(),
            _ => throw new Exception("Unsupported type")
        };

    private NumberValue ParseNumber()
    {
        var token = parser.Consume(TokenType.NumberLiteral).Value;

        if (token.EndsWith('f'))
        {
            var floatValue = float.Parse(token[..^1]);
            return new FloatValue(floatValue);
        }

        if (token.EndsWith('d'))
        {
            var doubleValue = double.Parse(token[..^1]);
            return new DoubleValue(doubleValue);
        }

        if (token.EndsWith('m'))
        {
            var decimalValue = decimal.Parse(token[..^1]);
            return new DecimalValue(decimalValue);
        }

        return new IntValue(int.Parse(token));
    }

    private BooleanValue ParseBool()
    {
        if (parser.Match(TokenType.BooleanTrueLiteral)) return new BooleanValue(true);
        if (parser.Match(TokenType.BooleanFalseLiteral)) return new BooleanValue(false);
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