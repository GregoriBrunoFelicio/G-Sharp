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

        return new LetExpression(bindingName, value) { Line = nameToken.Line, Column = nameToken.Column };
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
