using GSharp.Parser;
using FluentAssertions;
using GSharp.AST;
using GSharp.Lexer;

namespace G.Sharp.Compiler.Tests.Parser;

public class WhileParserTests
{
    [Fact]
    public void Should_Parse_Empty_While_Block()
    {
        var tokens = new List<Token>
        {
            new(TokenType.While, "while"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new WhileParser(parser).Parse();

        result.Condition.Should().BeOfType<LiteralExpression>();
        result.Body.Should().BeEmpty();
    }

    [Fact]
    public void Should_Parse_While_With_Boolean_Variable()
    {
        var tokens = new List<Token>
        {
            new(TokenType.While, "while"),
            new(TokenType.Identifier, "isReady"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens)
        {
            VariablesDeclared = { ["isReady"] = new GType(GPrimitiveType.Boolean) }
        };

        var result = new WhileParser(parser).Parse();
        result.Condition.Should().BeOfType<VariableExpression>();
    }

    [Fact]
    public void Should_Parse_While_With_Logical_Or()
    {
        var tokens = new List<Token>
        {
            new(TokenType.While, "while"),
            new(TokenType.Identifier, "x"),
            new(TokenType.LessThan, "<"),
            new(TokenType.NumberLiteral, "5"),
            new(TokenType.Or, "or"),
            new(TokenType.Identifier, "x"),
            new(TokenType.GreaterThan, ">"),
            new(TokenType.NumberLiteral, "10"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens)
        {
            VariablesDeclared = { ["x"] = new GType(GPrimitiveType.Number) }
        };

        var result = new WhileParser(parser).Parse();
        result.Condition.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void Should_Throw_When_Condition_Ends_With_Operator()
    {
        var tokens = new List<Token>
        {
            new(TokenType.While, "while"),
            new(TokenType.NumberLiteral, "10"),
            new(TokenType.EqualEqual, "=="),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var act = () => new WhileParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("*expression*");
    }

    [Fact]
    public void Should_Parse_While_With_Statement_Block_Containing_If()
    {
        var tokens = new List<Token>
        {
            new(TokenType.While, "while"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.LeftBrace, "{"),

            new(TokenType.If, "if"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),

            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new WhileParser(parser).Parse();

        result.Body.Should().ContainSingle().Which.Should().BeOfType<IfStatement>();
    }

    [Fact]
    public void Should_Throw_When_Condition_Has_Missing_RightOperand()
    {
        var tokens = new List<Token>
        {
            new(TokenType.While, "while"),
            new(TokenType.Identifier, "x"),
            new(TokenType.GreaterThan, ">"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens)
        {
            VariablesDeclared = { ["x"] = new GType(GPrimitiveType.Number) }
        };

        var act = () => new WhileParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("*expression*");
    }

    [Fact]
    public void Should_Throw_When_Missing_RightBrace()
    {
        var tokens = new List<Token>
        {
            new(TokenType.While, "while"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Println, "println"),
            new(TokenType.StringLiteral, "oops"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var act = () => new WhileParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("*Invalid statement*");
    }

    [Fact]
    public void Should_Throw_When_Missing_LeftBrace()
    {
        var tokens = new List<Token>
        {
            new(TokenType.While, "while"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.Println, "println"),
            new(TokenType.StringLiteral, "fail"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var act = () => new WhileParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("Expected token LeftBrace*");
    }
}