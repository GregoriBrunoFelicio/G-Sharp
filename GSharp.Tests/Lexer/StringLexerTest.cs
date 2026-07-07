using FluentAssertions;
using GSharp.Lexer;

namespace G.Sharp.Compiler.Tests.Lexer;

public class StringLexerTest
{
    [Fact]
    public void Should_Parse_Simple_String()
    {
        var lexer = new GSharp.Lexer.Lexer("\"hello\"");
        var token = lexer.ReadString();

        token.Type.Should().Be(TokenType.StringLiteral);
        token.Value.Should().Be("hello");
    }

    [Fact]
    public void Should_Parse_Empty_String()
    {
        var lexer = new GSharp.Lexer.Lexer("\"\"");
        var token = lexer.ReadString();

        token.Type.Should().Be(TokenType.StringLiteral);
        token.Value.Should().Be(string.Empty);
    }

    [Fact]
    public void Should_Throw_If_String_Is_Unterminated()
    {
        var lexer = new GSharp.Lexer.Lexer("\"unterminated string");

        var act = () => lexer.ReadString();

        act.Should()
            .Throw<Exception>()
            .WithMessage("*unterminated string literal");
    }

    [Fact]
    public void Should_Parse_String_With_Spaces()
    {
        var lexer = new GSharp.Lexer.Lexer("\"hello greg\"");
        var token = lexer.ReadString();

        token.Type.Should().Be(TokenType.StringLiteral);
        token.Value.Should().Be("hello greg");
    }

    [Fact]
    public void Should_Parse_String_With_Special_Characters()
    {
        var lexer = new GSharp.Lexer.Lexer("\"g$#@!&*()_+=<>[]{}\"");
        var token = lexer.ReadString();

        token.Type.Should().Be(TokenType.StringLiteral);
        token.Value.Should().Be("g$#@!&*()_+=<>[]{}");
    }

    [Fact]
    public void Should_Parse_String_With_Newlines_And_Tabs()
    {
        var lexer = new GSharp.Lexer.Lexer("\"line1\\nline2\\tend\"");
        var token = lexer.ReadString();

        token.Type.Should().Be(TokenType.StringLiteral);
        token.Value.Should().Be("line1\\nline2\\tend");
    }
}