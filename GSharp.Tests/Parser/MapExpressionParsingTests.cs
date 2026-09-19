using FluentAssertions;
using GSharp.Compiler.AST;

namespace G.Sharp.Compiler.Tests.Parser;

public class MapExpressionParsingTests
{
    private static MapExpression Parsed(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        return (MapExpression)((BindingExpression)expressions[0]).Value;
    }

    private static void ShouldThrow(string source, string expectedFragment)
    {
        var act = () =>
        {
            var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
            new GSharp.Compiler.Parser.Parser(tokens).Parse();
        };

        act.Should().Throw<Exception>().WithMessage($"*{expectedFragment}*");
    }

    [Fact]
    public void Parses_Key_Value_Pairs_Separated_By_Whitespace()
    {
        var map = Parsed("m -> {\"a\": 1 \"b\": 2}");

        map.Keys.Should().HaveCount(2);
        map.Values.Should().HaveCount(2);
        map.Keys[0].Should().BeOfType<LiteralExpression>().Which.Value.Should().Be("a");
        map.Values[0].Should().BeOfType<LiteralExpression>().Which.Value.Should().Be(1);
        map.Keys[1].Should().BeOfType<LiteralExpression>().Which.Value.Should().Be("b");
        map.Values[1].Should().BeOfType<LiteralExpression>().Which.Value.Should().Be(2);
    }

    [Fact]
    public void Empty_Map_Literal_Parses()
    {
        var map = Parsed("m -> {}");

        map.Keys.Should().BeEmpty();
        map.Values.Should().BeEmpty();
    }

    [Fact]
    public void Keys_And_Values_Can_Be_Arbitrary_Expressions()
    {
        var map = Parsed("m -> {\"sum\": 1 + 1 \"x\": x}");

        map.Values[0].Should().BeOfType<BinaryExpression>();
        map.Values[1].Should().BeOfType<IdentifierExpression>();
    }

    [Fact]
    public void Missing_Colon_Throws()
    {
        ShouldThrow("m -> {\"a\" 1}", "Colon");
    }

    [Fact]
    public void Missing_Closing_Brace_Throws()
    {
        ShouldThrow("m -> {\"a\": 1", "unexpected");
    }
}
