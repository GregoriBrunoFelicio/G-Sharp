using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers.Shared;
using static G.Sharp.Compiler.Parsers.Shared.Validations;
using Type = G.Sharp.Compiler.AST.Type;

namespace G.Sharp.Compiler.Parsers;

public class LetParser(ParserHelper parserHelper) : StatementParserBase(parserHelper), IStatementParser
{
    private readonly HashSet<string> _variablesDeclared = [];

    public Statement Parse()
    {
        var variableName = Consume(TokenType.Identifier).Value;

        ValidateVariableName(variableName);

        Consume(TokenType.Colon);

        var varType = GetVariableType();

        Consume(TokenType.Equals);

        var value = GetValue(varType);

        if (!IsTypeCompatible(varType, value))
        {
            throw new Exception($"Type mismatch: expected {varType}, but got {value.Type}");
        }

        Consume(TokenType.Semicolon);

        _variablesDeclared.Add(variableName);

        return new LetStatement(variableName, value);
    }

    private Type GetVariableType()
    {
        if (Match(TokenType.Number))
        {
            return Type.Number;
        }

        if (Match(TokenType.String))
        {
            return Type.String;
        }

        if (Match(TokenType.Boolean))
        {
            return Type.Boolean;
        }

        throw new Exception("Expected a type not found.");
    }

    private VariableValue GetValue(Type type) =>
        type switch
        {
            Type.Number => new NumberValue(int.Parse(Consume(TokenType.NumberLiteral).Value)),
            Type.String => new StringValue(Consume(TokenType.StringLiteral).Value),
            Type.Boolean => new BooleanValue(ConsumeBool()),
            _ => throw new Exception("Unsupported type")
        };

    private bool ConsumeBool()
    {
        if (Match(TokenType.BooleanTrueLiteral)) return true;
        if (Match(TokenType.BooleanFalseLiteral)) return false;
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