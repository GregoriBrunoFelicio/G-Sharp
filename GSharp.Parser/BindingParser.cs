using GSharp.AST;
using GSharp.Lexer;
using static GSharp.Parser.Validations;

namespace GSharp.Parser;

public class BindingParser(Parser parser)
{
    public BindingExpression Parse()
    {
        var (_, bindingName, line, column) = GetBindingNameToken();

        parser.Consume(TokenType.ThinArrow);
        var value = new ExpressionParser(parser).Parse();

        parser.DeclareBinding(bindingName);

        return new BindingExpression(bindingName, value) { Line = line, Column = column };
    }

    private Token GetBindingNameToken()
    {
        var token = parser.Identifier();
        ValidateBindingName(token);
        return token;
    }

    private static void ValidateBindingName(Token token)
    {
        if (!IsValidBindingName(token.Value))
            throw new Exception($"{token.Line}: invalid binding name '{token.Value}'");

        if (IsReserved(token.Value))
            throw new Exception($"'{token.Value}' is a reserved keyword.");
    }
}
