using FluentAssertions;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Lexer;

public class ArrayLexerTests
{
    [Fact]
    public void Should_Tokenize_NumberArray_Correctly()
    {
        var code = "let nums: number[] = [1 2 3 4 5 6 7 8 9];";
        var lexer = new Compiler.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();

        tokens.Should().HaveCount(20);
        tokens.Select(t => t.Type).Should().ContainInOrder(
            TokenType.Let,
            TokenType.Identifier,
            TokenType.Colon,
            TokenType.Number,
            TokenType.LeftBracket,
            TokenType.RightBracket,
            TokenType.Equals,
            TokenType.LeftBracket,
            TokenType.NumberLiteral,
            TokenType.NumberLiteral,
            TokenType.NumberLiteral,
            TokenType.NumberLiteral,
            TokenType.NumberLiteral,
            TokenType.RightBracket,
            TokenType.Semicolon,
            TokenType.EndOfFile
        );
    }

    [Fact]
    public void Should_Tokenize_StringArray_Correctly()
    {
        var code = "let names: string[] = [\"greg\" \"bruno\" \"felicio\"];";
        var lexer = new Compiler.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();

        tokens.Should().HaveCount(14);
        tokens.Select(t => t.Type).Should().ContainInOrder(
            TokenType.Let,
            TokenType.Identifier,
            TokenType.Colon,
            TokenType.String,
            TokenType.LeftBracket,
            TokenType.RightBracket,
            TokenType.Equals,
            TokenType.LeftBracket,
            TokenType.StringLiteral,
            TokenType.StringLiteral,
            TokenType.RightBracket,
            TokenType.Semicolon,
            TokenType.EndOfFile
        );
    }

    [Fact]
    public void Should_Tokenize_BooleanArray_Correctly()
    {
        var code = "let flags: bool[] = [true false true];";
        var lexer = new Compiler.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();

        tokens.Should().HaveCount(14);
        tokens.Select(t => t.Type).Should().ContainInOrder(
            TokenType.Let,
            TokenType.Identifier,
            TokenType.Colon,
            TokenType.Boolean,
            TokenType.LeftBracket,
            TokenType.RightBracket,
            TokenType.Equals,
            TokenType.LeftBracket,
            TokenType.BooleanTrueLiteral,
            TokenType.BooleanFalseLiteral,
            TokenType.BooleanTrueLiteral,
            TokenType.RightBracket,
            TokenType.Semicolon,
            TokenType.EndOfFile
        );
    }

    [Fact]
    public void Should_Throw_On_Invalid_Symbol()
    {
        var code = "let nums: number[] = [1 2 @ 3];";
        var lexer = new Compiler.Lexer.Lexer(code);

        var act = () => lexer.Tokenize();
        act.Should().Throw<Exception>().WithMessage("*Unexpected character*");
    }
}