using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;

namespace G.Sharp.Compiler.Tests.Parser;

public class ForParserTests
{
    [Fact]
    public void Should_Parse_If_With_Empty_Then_Block()
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

        result.ThenBody.Should().BeEmpty();
        result.ElseBody.Should().BeEmpty();
    }

    [Fact]
    public void Should_Parse_If_With_Empty_Else_Block()
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
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new IfParser(parser).Parse();

        result.ThenBody.Should().HaveCount(1);
        result.ElseBody.Should().BeEmpty();
    }

    [Fact]
    public void Should_Parse_Nested_If_Inside_Then_Block()
    {
        var tokens = new List<Token>
        {
            new(TokenType.If, "if"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.If, "if"),
            new(TokenType.BooleanFalseLiteral, "false"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Println, "println"),
            new(TokenType.StringLiteral, "nested"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new IfParser(parser).Parse();

        result.ThenBody.Should().HaveCount(1);
        result.ThenBody.First().Should().BeOfType<IfStatement>();
    }

    [Fact]
    public void Should_Throw_If_Else_Has_No_Braces()
    {
        var tokens = new List<Token>
        {
            new(TokenType.If, "if"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Println, "ok"),
            new(TokenType.StringLiteral, "yep"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.Else, "else"),
            new(TokenType.Println, "fail"),
            new(TokenType.StringLiteral, "nope"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => new IfParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("Expected token LeftBrace*");
    }

    [Fact]
    public void Should_Throw_When_Double_Else_Branches()
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
            new(TokenType.Else, "else"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        new IfParser(parser).Parse();

        var act = () => parser.ParseNextStatement();
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Should_Throw_If_Condition_Is_Missing()
    {
        var tokens = new List<Token>
        {
            new(TokenType.If, "if"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Println, "println"),
            new(TokenType.StringLiteral, "fail"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => new IfParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("Unexpected token in expression.");
    }

    [Fact]
    public void Should_Parse_Statement_Inside_For_Body()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "i"),
            new(TokenType.In, "in"),
            new(TokenType.NumberLiteral, "123"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Println, "println"),
            new(TokenType.StringLiteral, "hello"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new ForParser(parser).Parse();

        result.Variable.Should().Be("i");
        result.Iterable.Should().BeOfType<LiteralExpression>();
        result.Body.Should().ContainSingle();
        result.Body[0].Should().BeOfType<PrintStatement>();
    }
}