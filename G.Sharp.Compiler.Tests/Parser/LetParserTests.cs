using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Parser;

public class LetParserTests
{
    [Fact]
    public void Parse_Should_Let_Statement_With_Number()
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
    public void Parse_Should_Let_Statement_With_String()
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
    public void Parse_Should_Let_Statement_With_Bool_True()
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
    public void Parse_Should_Let_Statement_With_Bool_False()
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
    
    [Fact]
    public void Parse_Should_Let_Statement_With_NumberArray()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "numbers"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.LeftBracket, "["),
            new(TokenType.RightBracket, "]"),
            new(TokenType.Equals, "="),
            new(TokenType.LeftBracket, "["),
            new(TokenType.NumberLiteral, "1"),
            new(TokenType.NumberLiteral, "2"),
            new(TokenType.NumberLiteral, "3"),
            new(TokenType.RightBracket, "]"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        var let = result.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<LetStatement>()
            .Subject;

        let.VariableName.Should().Be("numbers");
        let.VariableValue.Should().BeOfType<ArrayValue>();

        var array = let.VariableValue.As<ArrayValue>();
        array.Elements.Should().HaveCount(3);
        array.Elements[0].As<IntValue>().Value.Should().Be(1);
        array.Elements[1].As<IntValue>().Value.Should().Be(2);
        array.Elements[2].As<IntValue>().Value.Should().Be(3);
    }
    
    [Fact]
    public void Parse_Should_Throw_When_Let_Statement_Assigns_Invalid_Value_To_NumberArray()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "broken"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.LeftBracket, "["),
            new(TokenType.RightBracket, "]"),
            new(TokenType.Equals, "="),
            new(TokenType.LeftBracket, "["),
            new(TokenType.StringLiteral, "not-a-number"),
            new(TokenType.RightBracket, "]"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);

        var act = () => parser.Parse();
       
        act.Should().Throw<Exception>()
            .WithMessage("Expected token NumberLiteral, got StringLiteral");
    }
}