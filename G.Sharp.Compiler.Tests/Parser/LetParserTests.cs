using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Parser;

public class LetParserTests
{
    [Fact]
    public void Should_Parse_Let_Statement_With_Number()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "42"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        var let = result.Should()
            .ContainSingle()
            .Which
            .Should()
            .BeOfType<LetStatement>()
            .Subject
            .As<LetStatement>();

        let.VariableName.Should().Be("x");
        let.VariableValue.Should().BeOfType<IntValue>();
        let.VariableValue.As<IntValue>().Value.Should().Be(42);
    }

    [Fact]
    public void Should_Parse_Let_Statement_With_String()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "name"),
            new(TokenType.Colon, ":"),
            new(TokenType.String, "string"),
            new(TokenType.Equals, "="),
            new(TokenType.StringLiteral, "Greg"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        var let = result.Should()
            .ContainSingle()
            .Which
            .Should()
            .BeOfType<LetStatement>()
            .Subject
            .As<LetStatement>();

        let.VariableName.Should().Be("name");
        let.VariableValue.Should().BeOfType<StringValue>();
        let.VariableValue.As<StringValue>().Value.Should().Be("Greg");
    }

    [Fact]
    public void Should_Parse_Let_Statement_With_Bool_True()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "isTrue"),
            new(TokenType.Colon, ":"),
            new(TokenType.Boolean, "bool"),
            new(TokenType.Equals, "="),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        var let = result.Should()
            .ContainSingle()
            .Which
            .Should()
            .BeOfType<LetStatement>()
            .Subject
            .As<LetStatement>();

        let.VariableName.Should().Be("isTrue");
        let.VariableValue.Should().BeOfType<BooleanValue>();
        let.VariableValue.As<BooleanValue>().Value.Should().Be(true);
    }

    [Fact]
    public void Should_Parse_Let_Statement_With_Bool_False()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "isFalse"),
            new(TokenType.Colon, ":"),
            new(TokenType.Boolean, "bool"),
            new(TokenType.Equals, "="),
            new(TokenType.BooleanFalseLiteral, "false"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        var let = result.Should()
            .ContainSingle()
            .Which
            .Should()
            .BeOfType<LetStatement>()
            .Subject
            .As<LetStatement>();

        let.VariableName.Should().Be("isFalse");
        let.VariableValue.Should().BeOfType<BooleanValue>();
        let.VariableValue.As<BooleanValue>().Value.Should().Be(false);
    }
}