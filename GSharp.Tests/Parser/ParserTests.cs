using FluentAssertions;
using GSharp.AST;
using GSharp.Lexer;

namespace G.Sharp.Compiler.Tests.Parser;

public class ParserTests
{
    [Fact]
    public void Should_Parse_Multiple_Statements()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "42"),
            new(TokenType.Semicolon, ";"),

            new(TokenType.Println, "println"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Semicolon, ";"),

            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);

        var result = parser.Parse();

        result.Should().HaveCount(2);
        result[0].Should().BeOfType<LetStatement>();
        result[1].Should().BeOfType<PrintStatement>();
    }

    [Fact]
    public void Should_Parse_Print_Statement()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Println, "println"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens)
        {
            VariablesDeclared =
            {
                ["x"] = new GType(GPrimitiveType.Number)
            }
        };

        var result = parser.Parse();

        result.Should().ContainSingle();
        result[0].Should().BeOfType<PrintStatement>();
    }

    [Fact]
    public void Should_Parse_Assignment_When_Identifier_Is_First()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "x"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "10"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens)
        {
            VariablesDeclared =
            {
                ["x"] = new GType(GPrimitiveType.Number)
            }
        };

        var result = parser.Parse();

        result.Should().ContainSingle();
        result[0].Should().BeOfType<AssignmentStatement>();
    }

    [Fact]
    public void Should_Parse_For_Loop()
    {
        var tokens = new List<Token>
        {
            new(TokenType.For, "for"),
            new(TokenType.Identifier, "i"),
            new(TokenType.In, "in"),
            new(TokenType.NumberLiteral, "123"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = parser.Parse();

        result.Should().ContainSingle();
        result[0].Should().BeOfType<ForStatement>();
    }

    [Fact]
    public void Should_Throw_When_Statement_Is_Invalid()
    {
        var tokens = new List<Token>
        {
            new(TokenType.String, "string"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);

        var action = () => parser.Parse();

        action.Should().Throw<Exception>()
            .WithMessage("Invalid statement string");
    }

    [Fact]
    public void Should_Consume_Token_When_Match_Is_True()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);

        var matched = parser.Match(TokenType.Let);

        matched.Should().BeTrue();
        parser.Previous().Type.Should().Be(TokenType.Let);
    }

    [Fact]
    public void Should_Not_Consume_When_Match_Is_False()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Println, "println"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);

        var matched = parser.Match(TokenType.Let);

        matched.Should().BeFalse();
        parser.Current().Type.Should().Be(TokenType.Println);
    }

    [Fact]
    public void Should_Throw_When_Consume_Expected_Token_Not_Found()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "x"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "42"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);

        parser.Consume(TokenType.Identifier);
        parser.Consume(TokenType.Equals);
        parser.Consume(TokenType.NumberLiteral);

        var action = () => parser.Consume(TokenType.Semicolon);

        action.Should().Throw<Exception>()
            .WithMessage("Expected token Semicolon, got EndOfFile");
    }

    [Fact]
    public void Should_Return_Previous_Token()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x")
        };

        var parser = new GSharp.Parser.Parser(tokens);

        parser.Match(TokenType.Let);
        parser.Match(TokenType.Identifier);

        var previous = parser.Previous();

        previous.Type.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void Should_Throw_When_Previous_Called_Before_Advance()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let")
        };

        var parser = new GSharp.Parser.Parser(tokens);

        var action = () => parser.Previous();

        action.Should().Throw<Exception>()
            .WithMessage("No previous token");
    }

    [Fact]
    public void Should_Throw_When_Current_Called_At_End()
    {
        var tokens = new List<Token>
        {
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        parser.Consume(TokenType.EndOfFile);

        var action = () => parser.Current();

        action.Should().Throw<Exception>()
            .WithMessage("Unexpected end of input.");
    }
}