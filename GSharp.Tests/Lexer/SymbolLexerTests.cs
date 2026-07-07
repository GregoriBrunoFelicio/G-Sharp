using FluentAssertions;
using GSharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Lexer;

public class SymbolLexerTests
{
    [Theory]
    [InlineData("->", TokenType.ThinArrow)]
    [InlineData(">=", TokenType.GreaterThanOrEqual)]
    [InlineData("<=", TokenType.LessThanOrEqual)]
    [InlineData("==", TokenType.EqualEqual)]
    [InlineData("!=", TokenType.NotEqual)]
    public void Should_Recognize_Composite_Symbols(string code, TokenType expected)
    {
        var lexer = new GSharp.Compiler.Lexer.Lexer(code);
        var token = lexer.ReadSymbol();

        token.Type.Should().Be(expected);
        token.Value.Should().Be(code);
    }

    [Theory]
    [InlineData(">", TokenType.GreaterThan)]
    [InlineData("<", TokenType.LessThan)]
    [InlineData("+", TokenType.Plus)]
    [InlineData("-", TokenType.Minus)]
    [InlineData("*", TokenType.Multiply)]
    [InlineData("/", TokenType.Divide)]
    public void Should_Recognize_Single_Symbols(string code, TokenType expected)
    {
        var lexer = new GSharp.Compiler.Lexer.Lexer(code);
        var token = lexer.ReadSymbol();

        token.Type.Should().Be(expected);
        token.Value.Should().Be(code);
    }

    [Fact]
    public void Should_Throw_When_Symbol_Is_Invalid()
    {
        var lexer = new GSharp.Compiler.Lexer.Lexer("@");

        var act = () => lexer.ReadSymbol();

        act.Should().Throw<Exception>()
            .WithMessage("1: unexpected '@'");
    }
}