using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Parser;

public class ParserTests
{
    [Fact]
    public void Parse_Should_Handle_Multiple_Statements()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "1"),
            new(TokenType.Semicolon, ";"),

            new(TokenType.Identifier, "x"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "2"),
            new(TokenType.Semicolon, ";"),

            new(TokenType.Println, "println"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Semicolon, ";"),

            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        result.Should().HaveCount(3);
        result[0].Should().BeOfType<LetStatement>();
        result[1].Should().BeOfType<AssignmentStatement>();
        result[2].Should().BeOfType<PrintStatement>();
    }
    
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
    
    [Fact]
    public void Should_Throw_When_Input_Is_Unexpectedly_Truncated()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => parser.Parse();

        act.Should().Throw<Exception>().WithMessage("*Expected token Equals*");
    }
    
    [Fact]
    public void Parse_Should_Ignore_Extra_EndOfFile_Tokens()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "5"),
            new(TokenType.Semicolon, ";"),

            new(TokenType.EndOfFile, ""),
            new(TokenType.EndOfFile, "") 
        };

        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        result.Should().HaveCount(1);
    }
}