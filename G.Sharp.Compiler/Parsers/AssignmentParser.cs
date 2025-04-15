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

        parser.Equals();
        var expression = new ExpressionParser(parser).Parse();

        var value = expression.GetLiteralValue();

        // IT is TERRIBLE!!!!
        if (!IsTypeCompatible(varType, value.Type))
            throw new Exception($"Type mismatch: expected {varType}, but got {value.Type}");

        parser.Semicolon();

        return new AssignmentStatement(variableName, expression);
    }
}