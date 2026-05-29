using GSharp.AST;
using GSharp.Lexer;
using static GSharp.Parser.Validations;

namespace GSharp.Parser;

public class LetParser(Parser parser)
{
    public LetExpression Parse()
    {
        parser.Consume(TokenType.Let);
        var bindingName = GetBindingName();

        parser.Equals();
        var value = new ExpressionParser(parser).Parse();

        parser.DeclaredBindings.Add(bindingName);

        return new LetExpression(bindingName, value);
    }

    private string GetBindingName()
    {
        var name = parser.Identifier().Value;
        ValidateBindingName(name);
        return name;
    }

    private void ValidateBindingName(string bindingName)
    {
        if (parser.DeclaredBindings.Contains(bindingName))
            throw new Exception($"Binding '{bindingName}' already declared.");

        if (!IsValidBindingName(bindingName))
        {
            var t = parser.Previous();
            throw new Exception($"{t.Line}: invalid binding name '{bindingName}'");
        }

        if (IsReserved(bindingName))
            throw new Exception($"'{bindingName}' is a reserved keyword.");
    }
}
