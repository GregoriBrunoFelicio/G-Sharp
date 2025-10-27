using FluentAssertions;
using GSharp.AST;
using GSharp.Lexer;
using GSharp.Parser;

namespace G.Sharp.Compiler.Tests.Parser;

public class ValueParserTests
{
    [Fact]
    public void Parse_With_String_Literal_And_String_Type_ReturnsStringValue()
    {
        var token = new Token(TokenType.StringLiteral, "hello");
        var parser = new GSharp.Parser.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(new GStringType());

        result.Should().BeOfType<StringValue>();
        result.As<StringValue>().Value.Should().Be("hello");
    }

    [Fact]
    public void Parse_WithNumberLiteralWithoutSuffix_ReturnsIntValue()
    {
        var token = new Token(TokenType.NumberLiteral, "42");
        var parser = new GSharp.Parser.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(new  GNumberType());

        result.Should().BeOfType<IntValue>();
        result.As<IntValue>().Value.Should().Be(42);
    }

    [Fact]
    public void Parse_WithNumberLiteralWithSuffixF_ReturnsFloatValue()
    {
        var token = new Token(TokenType.NumberLiteral, "3.14f");
        var parser = new GSharp.Parser.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(new GNumberType());

        result.Should().BeOfType<FloatValue>();
        result.As<FloatValue>().Value.Should().Be(3.14f);
    }

    [Fact]
    public void Parse_WithNumberLiteralWithSuffixD_ReturnsDoubleValue()
    {
        var token = new Token(TokenType.NumberLiteral, "2.71d");
        var parser = new GSharp.Parser.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(new GNumberType());

        result.Should().BeOfType<DoubleValue>();
        result.As<DoubleValue>().Value.Should().Be(2.71);
    }

    [Fact]
    public void Parse_WithNumberLiteralWithSuffixM_ReturnsDecimalValue()
    {
        var token = new Token(TokenType.NumberLiteral, "100.5m");
        var parser = new GSharp.Parser.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(new GNumberType());

        result.Should().BeOfType<DecimalValue>();
        result.As<DecimalValue>().Value.Should().Be(100.5m);
    }

    [Fact]
    public void Parse_WithBooleanTrueLiteral_ReturnsTrueBooleanValue()
    {
        var token = new Token(TokenType.BooleanTrueLiteral, "true");
        var parser = new GSharp.Parser.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(new GBooleanType());

        result.Should().BeOfType<BooleanValue>();
        result.As<BooleanValue>().Value.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithBooleanFalseLiteral_ReturnsFalseBooleanValue()
    {
        var token = new Token(TokenType.BooleanFalseLiteral, "false");
        var parser = new GSharp.Parser.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(new GBooleanType());

        result.Should().BeOfType<BooleanValue>();
        result.As<BooleanValue>().Value.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithInvalidTokenForBoolean_ThrowsException()
    {
        var token = new Token(TokenType.StringLiteral, "\"not-a-boolean\"");
        var parser = new GSharp.Parser.Parser([token]);
        var valueParser = new ValueParser(parser);

        var act = () => valueParser.Parse(new  GBooleanType());

        act.Should().Throw<Exception>().WithMessage("Expected boolean literal");
    }

    // [Fact]
    // public void Parse_WithUnsupportedType_ThrowsException()
    // {
    //     var parser = new GSharp.Parser.Parser([]);
    //     var valueParser = new ValueParser(parser);
    //
    //     var unsupportedType = new GType((GPrimitiveType)999);
    //
    //     var act = () => valueParser.Parse(unsupportedType);
    //
    //     act.Should().Throw<Exception>().WithMessage("Unsupported type");
    // }
    
    [Fact]
    public void Parse_Should_Parse_NumberArray()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "["),
            new(TokenType.NumberLiteral, "1"),
            new(TokenType.NumberLiteral, "2"),
            new(TokenType.NumberLiteral, "3"),
            new(TokenType.RightBracket, "]"),
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(new GArrayType(new GNumberType()));

        result.Should().BeOfType<ArrayValue>();

        var array = result.As<ArrayValue>();
        array.Elements.Should().HaveCount(3);
        array.Elements[0].As<IntValue>().Value.Should().Be(1);
        array.Elements[1].As<IntValue>().Value.Should().Be(2);
        array.Elements[2].As<IntValue>().Value.Should().Be(3);
    }
    
    [Fact]
    public void Parse_Should_Parse_StringArray()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "["),
            new(TokenType.StringLiteral, "a"),
            new(TokenType.StringLiteral, "b"),
            new(TokenType.StringLiteral, "c"),
            new(TokenType.RightBracket, "]"),
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(new GArrayType(new GStringType()));

        result.Should().BeOfType<ArrayValue>();

        var array = result.As<ArrayValue>();
        array.Elements.Should().HaveCount(3);
        array.Elements[0].As<StringValue>().Value.Should().Be("a");
        array.Elements[1].As<StringValue>().Value.Should().Be("b");
        array.Elements[2].As<StringValue>().Value.Should().Be("c");
    }
    
    [Fact]
    public void Parse_Should_Parse_BooleanArray()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "["),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.BooleanFalseLiteral, "false"),
            new(TokenType.RightBracket, "]"),
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(new GArrayType(new GBooleanType()));

        result.Should().BeOfType<ArrayValue>();

        var array = result.As<ArrayValue>();
        array.Elements.Should().HaveCount(2);
        array.Elements[0].As<BooleanValue>().Value.Should().BeTrue();
        array.Elements[1].As<BooleanValue>().Value.Should().BeFalse();
    }
    
    [Fact]
    public void Parse_Should_Throw_When_ArrayElement_Has_Wrong_Type()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "["),
            new(TokenType.StringLiteral, "wrong"),
            new(TokenType.RightBracket, "]"),
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var valueParser = new ValueParser(parser);

        var act = () => valueParser.Parse(new GArrayType(new GNumberType()));

        act.Should().Throw<Exception>().WithMessage("Expected token NumberLiteral, got StringLiteral");
    }
    
    [Fact]
    public void Parse_Should_Parse_EmptyArray()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "[   "),
            new(TokenType.RightBracket, "]"),
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(new GArrayType(new GStringType()));

        result.Should().BeOfType<ArrayValue>();
        result.As<ArrayValue>().Elements.Should().BeEmpty();
    }
    
    // [Fact]
    // public void Parse_Should_Throw_When_Type_Is_Unsupported()
    // {
    //     var parser = new GSharp.Parser.Parser([]);
    //     var valueParser = new ValueParser(parser);
    //
    //     var act = () => valueParser.Parse(new GNumberType()999));
    //
    //     act.Should().Throw<Exception>().WithMessage("Unsupported type");
    // }
    
    [Fact]
    public void Parse_Should_Throw_When_Trying_To_Parse_Array_Of_Array()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "["),
            new(TokenType.LeftBracket, "["),
            new(TokenType.NumberLiteral, "1"),
            new(TokenType.RightBracket, "]"),
            new(TokenType.RightBracket, "]"),
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var valueParser = new ValueParser(parser);

        var act = () => valueParser.Parse(new GArrayType(new GNumberType()));

        act.Should().Throw<Exception>().WithMessage("Expected token NumberLiteral, got LeftBracket");
    }
}