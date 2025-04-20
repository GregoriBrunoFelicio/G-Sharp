using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;

namespace G.Sharp.Compiler.Tests.Parser;

public class IfParserTests
{
    [Fact]
    public void Should_Parse_If_Without_Else()
    {
        var tokens = new List<Token>
        {
            new(TokenType.If, "if"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new IfParser(parser).Parse();

        result.Condition.Should().BeOfType<LiteralExpression>();
        result.ThenBody.Should().BeEmpty();
        result.ElseBody.Should().BeEmpty();
    }

    [Fact]
    public void Should_Parse_If_With_Else()
    {
        var tokens = new List<Token>
        {
            new(TokenType.If, "if"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.Else, "else"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new IfParser(parser).Parse();

        result.Condition.Should().BeOfType<LiteralExpression>();
        result.ThenBody.Should().BeEmpty();
        result.ElseBody.Should().BeEmpty();
    }

    [Fact]
    public void Should_Parse_If_With_Statements_In_Both_Branches()
    {
        var tokens = new List<Token>
        {
            new(TokenType.If, "if"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Println, "println"),
            new(TokenType.StringLiteral, "then"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.Else, "else"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Println, "println"),
            new(TokenType.StringLiteral, "else"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new IfParser(parser).Parse();

        result.ThenBody.Should().ContainSingle().Which.Should().BeOfType<PrintStatement>();
        result.ElseBody.Should().ContainSingle().Which.Should().BeOfType<PrintStatement>();
    }

    [Fact]
    public void Should_Throw_When_Missing_LeftBrace_In_Then()
    {
        var tokens = new List<Token>
        {
            new(TokenType.If, "if"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => new IfParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("Expected token LeftBrace, got RightBrace");
    }

    [Fact]
    public void Should_Throw_When_Missing_RightBrace_In_Then()
    {
        var tokens = new List<Token>
        {
            new(TokenType.If, "if"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Println, "println"),
            new(TokenType.StringLiteral, "oops"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => new IfParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("Invalid statement");
    }
}