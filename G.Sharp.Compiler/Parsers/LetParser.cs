using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using static G.Sharp.Compiler.Parsers.Validations;

namespace G.Sharp.Compiler.Parsers;

public class LetParser(Parser parser)
{
    public Statement Parse()
    {
        var variableName = parser.Consume(TokenType.Identifier).Value;

        ValidateVariableName(variableName);

        parser.Consume(TokenType.Colon);

        var varType = GetVariableType();

        parser.Consume(TokenType.Equals);

        var valueParser = new ValueParser(parser);
        var value = valueParser.Parse(varType);

        if (!IsTypeCompatible(varType, value))
        {
            throw new Exception($"Type mismatch: expected {varType}, but got {value.Type}");
        }

        parser.Consume(TokenType.Semicolon);

        parser.VariablesDeclared.Add(variableName, varType);

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