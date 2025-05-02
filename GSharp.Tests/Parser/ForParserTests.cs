using FluentAssertions;
using GSharp.Parser;
using GSharp.AST;
using GSharp.Lexer;

namespace G.Sharp.Compiler.Tests.Parser;

public class ForParserTests
{
    [Fact]
    public void Should_Parse_Valid_For_Loop()
    {
        var tokens = new List<Token>
        {
            new(TokenType.For, "for"),
            new(TokenType.Identifier, "item"),
            new(TokenType.In, "in"),
            new(TokenType.Identifier, "nums"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Println, "println"),
            new(TokenType.Identifier, "item"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new ForParser(parser).Parse();

        result.Variable.Should().Be("item");
        result.Iterable.Should().BeOfType<VariableExpression>();
        result.Body.Should().ContainSingle().Which.Should().BeOfType<PrintStatement>();
    }

    [Fact]
    public void Should_Throw_When_Missing_In()
    {
        var tokens = new List<Token>
        {
            new(TokenType.For, "for"),
            new(TokenType.Identifier, "item"),
            new(TokenType.Identifier, "nums"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var act = () => new ForParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("*Expected token In*");
    }

    [Fact]
    public void Should_Throw_When_Missing_LeftBrace()
    {
        var tokens = new List<Token>
        {
            new(TokenType.For, "for"),
            new(TokenType.Identifier, "item"),
            new(TokenType.In, "in"),
            new(TokenType.Identifier, "nums"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var act = () => new ForParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("*Expected token LeftBrace*");
    }

    [Fact]
    public void Should_Parse_For_With_Empty_Body()
    {
        var tokens = new List<Token>
        {
            new(TokenType.For, "for"),
            new(TokenType.Identifier, "item"),
            new(TokenType.In, "in"),
            new(TokenType.Identifier, "nums"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new ForParser(parser).Parse();

        result.Variable.Should().Be("item");
        result.Iterable.Should().BeOfType<VariableExpression>();
        result.Body.Should().BeEmpty();
    }
}
