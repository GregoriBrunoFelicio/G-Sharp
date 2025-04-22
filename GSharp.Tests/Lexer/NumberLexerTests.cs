using FluentAssertions;
using GSharp.Lexer;

namespace G.Sharp.Compiler.Tests.Lexer;

public class NumberLexerTests
{
    [Theory]
    [InlineData("42", "42")]
    [InlineData("1234567890", "1234567890")]
    public void Should_Parse_Whole_Number(string input, string expected)
    {
        var lexer = new GSharp.Lexer.Lexer(input);
        var token = NumberLexer.Read(lexer);

        token.Type.Should().Be(TokenType.NumberLiteral);
        token.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("3.14", "3.14")]
    [InlineData("0.0", "0.0")]
    [InlineData("10.1", "10.1")]
    public void Should_Parse_Float_Number(string input, string expected)
    {
        var lexer = new GSharp.Lexer.Lexer(input);
        var token = NumberLexer.Read(lexer);

        token.Type.Should().Be(TokenType.NumberLiteral);
        token.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("1.2f", "1.2f")]
    [InlineData("3.1415d", "3.1415d")]
    [InlineData("99.99m", "99.99m")]
    public void Should_Parse_Number_With_Suffix(string input, string expected)
    {
        var lexer = new GSharp.Lexer.Lexer(input);
        var token = NumberLexer.Read(lexer);

        token.Type.Should().Be(TokenType.NumberLiteral);
        token.Value.Should().Be(expected);
    }

    [Fact]
    public void Should_Parse_Number_With_Dot_But_No_Digits_After()
    {
        var lexer = new GSharp.Lexer.Lexer("10.");
        var token = NumberLexer.Read(lexer);

        token.Type.Should().Be(TokenType.NumberLiteral);
        token.Value.Should().Be("10.");
    }

    [Fact]
    public void Should_Parse_Number_With_Dot_And_Stop_At_NonSuffix()
    {
        var lexer = new GSharp.Lexer.Lexer("10.5x");
        var token = NumberLexer.Read(lexer);

        token.Type.Should().Be(TokenType.NumberLiteral);
        token.Value.Should().Be("10.5");
        lexer.Current.Should().Be('x');
    }

    [Fact]
    public void Should_Parse_Number_With_Suffix_At_End_Of_Code()
    {
        var lexer = new GSharp.Lexer.Lexer("42.0f");
        var token = NumberLexer.Read(lexer);

        token.Type.Should().Be(TokenType.NumberLiteral);
        token.Value.Should().Be("42.0f");
        lexer.IsAtEnd().Should().BeTrue();
    }

    [Fact]
    public void Should_Parse_Number_And_Leave_Next_Token_Untouched()
    {
        var lexer = new GSharp.Lexer.Lexer("10.2f;");
        var token = NumberLexer.Read(lexer);

        token.Type.Should().Be(TokenType.NumberLiteral);
        token.Value.Should().Be("10.2f");
        lexer.Current.Should().Be(';');
    }
}