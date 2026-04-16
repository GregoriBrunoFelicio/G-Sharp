using GSharp.AST;

namespace GSharp.Parser;

public class AssignmentParser(Parser parser)
{
    public AssignmentStatement Parse()
    {
        var variableName = parser.ExpectDeclaredIdentifier();

        parser.Equals();

        var expression = new ExpressionParser(parser).Parse();

        return new AssignmentStatement(variableName, expression);
    }
}