using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Parser;

public class ParserTests
{
    [Fact]
    public void Should_Throw_When_Type_Is_Missing_In_Let()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "10"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => parser.Parse();

        act.Should().Throw<Exception>().WithMessage("*Expected token Colon*");
    }

    [Fact]
    public void Should_Throw_When_Statement_Is_Invalid()
    {
        var tokens = new List<Token>
        {
            new(TokenType.NumberLiteral, "99"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => parser.Parse();

        act.Should().Throw<Exception>().WithMessage("*Invalid statement*");
    }
}