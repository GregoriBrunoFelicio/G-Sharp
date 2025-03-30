using static G.Sharp.Compiler.Lexer.Syntax;

namespace G.Sharp.Compiler.Lexer;

public static class IdentifierLexer
{
    public static Token Read(Lexer lexer)
    {
        var start = lexer.Position;

        lexer.AdvanceWhile(char.IsLetterOrDigit);

        var value = lexer.Code[start..lexer.Position];

        var type = KeywordTokenMap.GetValueOrDefault(value, TokenType.Identifier);

        return new Token(type, value);
    }
}