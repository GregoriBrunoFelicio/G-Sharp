using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;


namespace G.Sharp.Compiler.Tests.Parser;

public class ExpressionParserTests
{
    [Fact]
    public void ExpressionParser_Should_Parse_Int_Literal_Expression()
    {
        var tokens = new List<Token>
        {
            new(TokenType.NumberLiteral, "123"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<LiteralExpression>();
        var literal = (LiteralExpression)result;
        literal.Value.Should().BeOfType<IntValue>();
        literal.Value.As<IntValue>().Value.Should().Be(123);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Float_Literal_Expression()
    {
        var tokens = new List<Token>
        {
            new(TokenType.NumberLiteral, "3.14f"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<LiteralExpression>();
        var literal = (LiteralExpression)result;
        literal.Value.Should().BeOfType<FloatValue>();
        literal.Value.As<FloatValue>().Value.Should().Be(3.14f);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_String_Literal_Expression()
    {
        var tokens = new List<Token>
        {
            new(TokenType.StringLiteral, "greg"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<LiteralExpression>();
        var literal = (LiteralExpression)result;
        literal.Value.Should().BeOfType<StringValue>();
        literal.Value.As<StringValue>().Value.Should().Be("greg");
    }

    [Theory]
    [InlineData(TokenType.BooleanTrueLiteral, true)]
    [InlineData(TokenType.BooleanFalseLiteral, false)]
    public void ExpressionParser_Should_Parse_Boolean_Literal_Expression(TokenType tokenType, bool expected)
    {
        var tokens = new List<Token>
        {
            new(tokenType, expected.ToString().ToLower()),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<LiteralExpression>();
        var literal = (LiteralExpression)result;
        literal.Value.Should().BeOfType<BooleanValue>();
        literal.Value.As<BooleanValue>().Value.Should().Be(expected);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Array_Of_Ints_Without_Commas()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "["),
            new(TokenType.NumberLiteral, "1"),
            new(TokenType.NumberLiteral, "2"),
            new(TokenType.NumberLiteral, "3"),
            new(TokenType.RightBracket, "]"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<LiteralExpression>();
        var array = result.As<LiteralExpression>().Value.As<ArrayValue>();

        array.Elements.Should().HaveCount(3);
        array.Elements[0].Should().BeOfType<IntValue>().Which.Value.Should().Be(1);
        array.Elements[1].Should().BeOfType<IntValue>().Which.Value.Should().Be(2);
        array.Elements[2].Should().BeOfType<IntValue>().Which.Value.Should().Be(3);
    }

    [Fact]
    public void ExpressionParser_Should_Throw_When_Array_Is_Empty()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "["),
            new(TokenType.RightBracket, "]"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => new ExpressionParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("Empty arrays are not supported.");
    }

    [Fact]
    public void ExpressionParser_Should_Throw_When_Array_Type_Cannot_Be_Inferred()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "["),
            new(TokenType.Identifier, "foo"),
            new(TokenType.RightBracket, "]"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => new ExpressionParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("Unable to infer array element type.");
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Variable_Expression()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "myVar"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<VariableExpression>();
        result.As<VariableExpression>().Name.Should().Be("myVar");
    }

    [Fact]
    public void ExpressionParser_Should_Throw_When_Token_Is_Invalid()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Equals, "="),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => new ExpressionParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("Unexpected token in expression.");
    }
}
