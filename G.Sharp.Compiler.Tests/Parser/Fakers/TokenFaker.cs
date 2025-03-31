using AutoBogus;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Parser.Fakers;

public class TokenFaker : AutoFaker<Token>
{
    public Token StringLiteral(string value = "abc") =>
        new(TokenType.StringLiteral, value);

    public Token NumberLiteral(string value) =>
        new(TokenType.NumberLiteral, value);

    public Token BooleanTrueLiteral() =>
        new(TokenType.BooleanTrueLiteral, "true");

    public Token BooleanFalseLiteral() =>
        new(TokenType.BooleanFalseLiteral, "false");
}