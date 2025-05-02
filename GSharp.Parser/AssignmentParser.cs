using GSharp.AST;
using static GSharp.Parser.Validations;

namespace GSharp.Parser;

public class AssignmentParser(Parser parser)
{
    public AssignmentStatement Parse()
    {
        var variableName = parser.ExpectDeclaredIdentifier();

        parser.Equals();

        var expression = new ExpressionParser(parser).Parse();

        parser.Semicolon();

        return new AssignmentStatement(variableName, expression);
    }
}