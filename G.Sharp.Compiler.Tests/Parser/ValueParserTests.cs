using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;

namespace G.Sharp.Compiler.Tests.Parser;

public class ValueParserTests
{
    [Fact]
    public void Parse_stringLiteral_returns_StringValue()
    {
        var token = new Token(TokenType.StringLiteral, "hello");
        var parser = new Parsers.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(GType.String);

        result.Should().BeOfType<StringValue>();
        result.As<StringValue>().Value.Should().Be("hello");
    }

    [Fact]
    public void Parse_numberLiteral_withoutSuffix_returns_IntValue()
    {
        var token = new Token(TokenType.NumberLiteral, "42");
        var parser = new Parsers.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(GType.Number);

        result.Should().BeOfType<IntValue>();
        result.As<IntValue>().Value.Should().Be(42);
    }

    [Fact]
    public void Parse_numberLiteral_withSuffixF_returns_FloatValue()
    {
        var token = new Token(TokenType.NumberLiteral, "3.14f");
        var parser = new Parsers.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(GType.Number);

        result.Should().BeOfType<FloatValue>();
        result.As<FloatValue>().Value.Should().Be(3.14f);
    }

    [Fact]
    public void Parse_numberLiteral_withSuffixD_returns_DoubleValue()
    {
        var token = new Token(TokenType.NumberLiteral, "2.71d");
        var parser = new Parsers.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(GType.Number);

        result.Should().BeOfType<DoubleValue>();
        result.As<DoubleValue>().Value.Should().Be(2.71);
    }

    [Fact]
    public void Parse_numberLiteral_withSuffixM_returns_DecimalValue()
    {
        var token = new Token(TokenType.NumberLiteral, "100.5m");
        var parser = new Parsers.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(GType.Number);

        result.Should().BeOfType<DecimalValue>();
        result.As<DecimalValue>().Value.Should().Be(100.5m);
    }

    [Fact]
    public void Parse_booleanTrueLiteral_returns_TrueBooleanValue()
    {
        var token = new Token(TokenType.BooleanTrueLiteral, "true");
        var parser = new Parsers.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(GType.Boolean);

        result.Should().BeOfType<BooleanValue>();
        result.As<BooleanValue>().Value.Should().BeTrue();
    }

    [Fact]
    public void Parse_booleanFalseLiteral_returns_FalseBooleanValue()
    {
        var token = new Token(TokenType.BooleanFalseLiteral, "false");
        var parser = new Parsers.Parser([token]);
        var valueParser = new ValueParser(parser);

        var result = valueParser.Parse(GType.Boolean);

        result.Should().BeOfType<BooleanValue>();
        result.As<BooleanValue>().Value.Should().BeFalse();
    }

    [Fact]
    public void Parse_booleanLiteral_invalidToken_throwsException()
    {
        var token = new Token(TokenType.StringLiteral, "\"not-a-boolean\"");
        var parser = new Parsers.Parser([token]);
        var valueParser = new ValueParser(parser);

        var act = () => valueParser.Parse(GType.Boolean);

        act.Should().Throw<Exception>().WithMessage("Expected boolean literal");
    }

    [Fact]
    public void Parse_unsupportedType_throwsException()
    {
        var parser = new Parsers.Parser([]);
        var valueParser = new ValueParser(parser);

        var act = () => valueParser.Parse((GType)999);

        act.Should().Throw<Exception>().WithMessage("Unsupported type");
    }
}