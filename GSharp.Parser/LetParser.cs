using GSharp.AST;
using GSharp.Lexer;
using static GSharp.Parser.Validations;

namespace GSharp.Parser;

public class LetParser(Parser parser)
{
    public LetStatement Parse()
    {
        parser.Consume(TokenType.Let);
        var bindingName = GetBindingName();

        parser.Equals();
        var expression = new ExpressionParser(parser).Parse();

        parser.DeclaredBindings.Add(bindingName);

        return new LetStatement(bindingName, expression);
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
            throw new Exception($"Invalid binding name: {bindingName}");

        if (IsReserved(bindingName))
            throw new Exception($"'{bindingName}' is a reserved keyword.");
    }
}