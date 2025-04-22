using FluentAssertions;
using GSharp.Lexer;

namespace G.Sharp.Compiler.Tests.Lexer;

public class SymbolLexerTests
{
    [Theory]
    [InlineData(">=", TokenType.GreaterThanOrEqual)]
    [InlineData("<=", TokenType.LessThanOrEqual)]
    [InlineData("==", TokenType.EqualEqual)]
    [InlineData("!=", TokenType.NotEqual)]
    public void Should_Recognize_Composite_Symbols(string code, TokenType expected)
    {
        var lexer = new GSharp.Lexer.Lexer(code);
        var token = SymbolLexer.Read(lexer);

        token.Type.Should().Be(expected);
        token.Value.Should().Be(code);
    }

    [Theory]
    [InlineData(">", TokenType.GreaterThan)]
    [InlineData("<", TokenType.LessThan)]
    [InlineData("=", TokenType.Equals)]
    [InlineData(":", TokenType.Colon)]
    [InlineData(";", TokenType.Semicolon)]
    [InlineData("+", TokenType.Plus)]
    [InlineData("-", TokenType.Minus)]
    [InlineData("*", TokenType.Multiply)]
    public void Should_Recognize_Single_Symbols(string code, TokenType expected)
    {
        var lexer = new GSharp.Lexer.Lexer(code);
        var token = SymbolLexer.Read(lexer);

        token.Type.Should().Be(expected);
        token.Value.Should().Be(code);
    }

    [Fact]
    public void Should_Throw_When_Symbol_Is_Invalid()
    {
        var lexer = new GSharp.Lexer.Lexer("@");

        var act = () => SymbolLexer.Read(lexer);

        act.Should().Throw<Exception>()
            .WithMessage("Unexpected symbol: '@'");
    }
}