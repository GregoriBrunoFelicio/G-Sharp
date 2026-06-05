using GSharp.AST;
using GSharp.Lexer;
using static GSharp.Parser.Validations;

namespace GSharp.Parser;

public class LetParser(Parser parser)
{
    public LetExpression Parse()
    {
        parser.Consume(TokenType.Let);
        var nameToken   = GetBindingNameToken();
        var bindingName = nameToken.Value;

        parser.Equals();
        var value = new ExpressionParser(parser).Parse();

        parser.DeclareBinding(bindingName);

        // Span points at the bound name so hovering it reports the binding's type.
        return new LetExpression(bindingName, value) { Line = nameToken.Line, Column = nameToken.Column };
    }

    private Token GetBindingNameToken()
    {
        var token = parser.Identifier();
        ValidateBindingName(token.Value);
        return token;
    }

    private void ValidateBindingName(string bindingName)
    {
        if (parser.IsDeclaredInCurrentScope(bindingName))
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
