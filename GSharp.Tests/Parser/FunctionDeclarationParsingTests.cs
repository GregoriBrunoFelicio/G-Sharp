using FluentAssertions;
using GSharp.Compiler.AST;

namespace G.Sharp.Compiler.Tests.Parser;

// `=>` marks where a function body starts for every function, named or anonymous, inline or
// block — there is no bare `name params\n    body` form. Breaking grammar change made 2026-09-02
// so `=>` always announces "body starts here," instead of block form being triggered solely by
// an indentation increase.
public class FunctionDeclarationParsingTests
{
    private static List<Expression> Parse(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        return new GSharp.Compiler.Parser.Parser(tokens).Parse();
    }

    [Fact]
    public void Block_Form_Without_Arrow_Is_A_Parse_Error()
    {
        var source =
            "max a b\n" +
            "    if a >= b then a else b";

        var act = () => Parse(source);

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Block_Form_With_Arrow_Parses_Correctly()
    {
        var source =
            "max a b =>\n" +
            "    if a >= b then a else b";

        var fn = Parse(source)[0].Should().BeOfType<FunctionDeclaration>().Subject;

        fn.Name.Should().Be("max");
        fn.Parameters.Should().Equal("a", "b");
        fn.Body.Should().ContainSingle().Which.Should().BeOfType<IfExpression>();
    }

    [Fact]
    public void Inline_Form_Still_Parses_Correctly()
    {
        var fn = Parse("square x => x * x")[0].Should().BeOfType<FunctionDeclaration>().Subject;

        fn.Parameters.Should().Equal("x");
        fn.Body.Should().ContainSingle().Which.Should().BeOfType<BinaryExpression>();
    }
}
