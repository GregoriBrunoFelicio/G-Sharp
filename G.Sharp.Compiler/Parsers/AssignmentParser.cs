using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using static G.Sharp.Compiler.Parsers.Validations;

namespace G.Sharp.Compiler.Parsers;

public class AssignmentParser(Parser parser)
{
    public AssignmentStatement Parse()
    {
        var variableName = parser.Identifier().Value;

        if (!parser.VariablesDeclared.TryGetValue(variableName, out var varType))
            throw new Exception($"Variable '{variableName}' is not declared.");

        var value = GetVariableValue(varType);
        
        if (!IsTypeCompatible(varType, value))
            throw new Exception($"Type mismatch: expected {varType}, but got {value.Type}");

        parser.Semicolon();

        return new AssignmentStatement(variableName, value);
    }

    private VariableValue GetVariableValue(GType type)
    {
        parser.Equals();
        var valueParser = new ValueParser(parser);
        return valueParser.Parse(type);
    }
}