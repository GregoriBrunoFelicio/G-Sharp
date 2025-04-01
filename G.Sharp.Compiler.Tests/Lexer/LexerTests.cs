using FluentAssertions;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Lexer;

public class LexerTests
{
    [Fact]
    public void Should_Tokenize_Simple_Variable_Declaration()
    {
        const string code = "let age: number = 20;";
        var lexer = new Sharp.Compiler.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();
        var expectedTokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "age"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "20"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        expectedTokens.Should().Equal(tokens);
    }

    [Fact]
    public void Should_Ignore_Whitespace_When_Tokenizing()
    {
        const string code = "let   age\n" +
                            ":\n" +
                            "number   = 20\n" +
                            ";\n";
        var lexer = new Sharp.Compiler.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();
        var expectedTokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "age"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "20"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        expectedTokens.Should().Equal(tokens);
    }

    [Theory]
    [InlineData("number", TokenType.Number)]
    [InlineData("string", TokenType.String)]
    [InlineData("println", TokenType.Println)]
    public void Should_Return_Correct_TokenType_For_Type_Or_Builtin_Keywords(string code, TokenType expectedTokenType)
    {
        var lexer = new Sharp.Compiler.Lexer.Lexer(code);
        var tokens = lexer.Tokenize().ToList();

        tokens[0].Type.Should().Be(expectedTokenType);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Throw_Exception_When_Code_Is_Empty_Or_Whitespace(string code)
    {
        var action = () => new Sharp.Compiler.Lexer.Lexer(code);

        action.Should()
            .Throw<NullReferenceException>()
            .WithMessage("Code cannot be null or empty.");
    }
    
    
}