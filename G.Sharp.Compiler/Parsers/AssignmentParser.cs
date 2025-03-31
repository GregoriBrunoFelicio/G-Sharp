using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using static G.Sharp.Compiler.Parsers.Validations;

namespace G.Sharp.Compiler.Parsers;

public class AssignmentParser(Parser parser)
{
    public Statement Parse()
    {
        var variableName = parser.Consume(TokenType.Identifier).Value;

        if (!parser.VariablesDeclared.TryGetValue(variableName, out var varType))
            throw new Exception($"Variable '{variableName}' is not declared.");

        parser.Consume(TokenType.Equals);

        var valueParser = new ValueParser(parser);
        var value = valueParser.Parse(varType);

        if (!IsTypeCompatible(varType, value))
            throw new Exception($"Type mismatch: expected {varType}, but got {value.Type}");

        parser.Consume(TokenType.Semicolon);

        return new AssignmentStatement(variableName, value);
    }
}