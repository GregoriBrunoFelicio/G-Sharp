using FluentAssertions;
using GSharp.Compiler.AST;

namespace G.Sharp.Compiler.Tests.Parser;

public class MatchExpressionParsingTests
{
    private static MatchExpression Parsed(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        return (MatchExpression)((BindingExpression)expressions[0]).Value;
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
    public void Parses_Literal_Arms_And_A_Trailing_Catch_All()
    {
        var match = Parsed("x -> match 1\n    1 => \"one\"\n    2 => \"two\"\n    n => \"other\"");

        match.Arms.Should().HaveCount(3);
        match.Arms[0].Pattern.Should().BeOfType<LiteralExpression>().Which.Value.Should().Be(1);
        match.Arms[1].Pattern.Should().BeOfType<LiteralExpression>().Which.Value.Should().Be(2);
        match.Arms[2].Pattern.Should().BeOfType<IdentifierExpression>().Which.Name.Should().Be("n");
    }

    [Fact]
    public void Catch_All_Not_Last_Throws()
    {
        ShouldThrow(
            "x -> match 1\n    n => \"catch-all\"\n    1 => \"one\"",
            "must be the last arm");
    }

    [Fact]
    public void Missing_Catch_All_Throws()
    {
        ShouldThrow(
            "x -> match 1\n    1 => \"one\"\n    2 => \"two\"",
            "requires a catch-all arm");
    }

    [Fact]
    public void Binary_Expression_Pattern_Throws()
    {
        ShouldThrow(
            "x -> match 1\n    1 + 1 => \"two\"\n    n => \"other\"",
            "Arrow");
    }

    [Fact]
    public void Array_Literal_Pattern_Throws()
    {
        ShouldThrow(
            "x -> match 1\n    [1] => \"arr\"\n    n => \"other\"",
            "expected a literal or identifier pattern");
    }

    [Fact]
    public void Sibling_Arms_Can_Reuse_The_Same_Pattern_Name_Across_Different_Matches()
    {
        var act = () => Parsed(
            "x -> match 1\n    n => n\n" +
            "y -> match 2\n    n => n");

        act.Should().NotThrow();
    }
}
