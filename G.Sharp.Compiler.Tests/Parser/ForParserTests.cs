using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;

namespace G.Sharp.Compiler.Tests.Parser;

public class ForParserTests
{
    [Fact]
    public void Should_Return_ForStatement_If_All_Is_Valid()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "item"),
            new(TokenType.In, "in"),
            new(TokenType.NumberLiteral, "123"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new ForParser(parser).Parse();

        result.Variable.Should().Be("item");
        result.Iterable.Should().BeOfType<LiteralExpression>();
        result.Body.Should().BeEmpty();
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