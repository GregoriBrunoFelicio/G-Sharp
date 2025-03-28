using FluentAssertions;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Lexer;

public class LexerStringTest
{
    [Fact]
    public void Should_Tokenize_Simple_String_Literal()
    {
        var lexer = new Sharp.Compiler.Lexer.Lexer("\"hello\"");
        var tokens = lexer.Tokenize().ToList();

        tokens.Should().HaveCount(2);
        tokens[0].Type.Should().Be(TokenType.StringLiteral);
        tokens[0].Value.Should().Be("hello");
    }
    
    [Fact]
    public void Should_Tokenize_Empty_String()
    {
        var lexer = new Sharp.Compiler.Lexer.Lexer("\"\"");
        var tokens = lexer.Tokenize().ToList();

        tokens.Should().ContainSingle(t => t.Type == TokenType.StringLiteral && t.Value == "");
    }
    
    [Fact]
    public void Should_Throw_On_Unterminated_String()
    {
        var lexer = new Sharp.Compiler.Lexer.Lexer("\"missing end");
        var act = () => lexer.Tokenize().ToList();

        act.Should()
            .Throw<Exception>()
            .WithMessage("Unterminated string literal. Expected closing '\"'.");
    }
    
    [Fact]
    public void Should_Tokenize_String_With_Special_Characters()
    {
        var lexer = new Sharp.Compiler.Lexer.Lexer("\"Hello, world! 123 #@!\"");
        var tokens = lexer.Tokenize().ToList();

        tokens.Should().ContainSingle(t =>
            t.Type == TokenType.StringLiteral &&
            t.Value == "Hello, world! 123 #@!");
    }
}