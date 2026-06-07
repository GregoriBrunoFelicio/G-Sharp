using FluentAssertions;
using GSharp.Lexer;

namespace G.Sharp.Compiler.Tests.Lexer;

public class ArrayLexerTests
{
    [Fact]
    public void Should_Tokenize_NumberArray_Correctly()
    {
        var code = "let nums = [1 2 3 4 5 6 7 8 9]";
        var lexer = new GSharp.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();

        tokens.Select(t => t.Type).Should().ContainInOrder(
            TokenType.Let,
            TokenType.Identifier,
            TokenType.Equals,
            TokenType.LeftBracket,
            TokenType.NumberLiteral,
            TokenType.NumberLiteral,
            TokenType.NumberLiteral,
            TokenType.NumberLiteral,
            TokenType.NumberLiteral,
            TokenType.RightBracket,
            TokenType.EndOfFile
        );
    }

    [Fact]
    public void Should_Tokenize_StringArray_Correctly()
    {
        var code = "let names = [\"greg\" \"bruno\" \"felicio\"]";
        var lexer = new GSharp.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();

        tokens.Select(t => t.Type).Should().ContainInOrder(
            TokenType.Let,
            TokenType.Identifier,
            TokenType.Equals,
            TokenType.LeftBracket,
            TokenType.StringLiteral,
            TokenType.StringLiteral,
            TokenType.StringLiteral,
            TokenType.RightBracket,
            TokenType.EndOfFile
        );
    }

    [Fact]
    public void Should_Tokenize_BooleanArray_Correctly()
    {
        var code = "let flags = [true false true]";
        var lexer = new GSharp.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();

        tokens.Select(t => t.Type).Should().ContainInOrder(
            TokenType.Let,
            TokenType.Identifier,
            TokenType.Equals,
            TokenType.LeftBracket,
            TokenType.BooleanTrueLiteral,
            TokenType.BooleanFalseLiteral,
            TokenType.BooleanTrueLiteral,
            TokenType.RightBracket,
            TokenType.EndOfFile
        );
    }

    [Fact]
    public void Should_Throw_On_Invalid_Symbol()
    {
        var code = "let nums = [1 2 @ 3]";
        var lexer = new GSharp.Lexer.Lexer(code);

        var act = () => lexer.Tokenize();
        act.Should().Throw<Exception>().WithMessage("*unexpected '@'");
    }
}
