using FluentAssertions;
using GSharp.Compiler.AST;
using GSharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Parser;

public class LambdaExpressionParsingTests
{
    private static List<Expression> Parse(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        return new GSharp.Compiler.Parser.Parser(tokens).Parse();
    }

    private static Expression BoundValue(string source) =>
        ((BindingExpression)Parse(source)[0]).Value;

    [Fact]
    public void Single_Parameter_Lambda_Bound_To_A_Name_Parses()
    {
        var lambda = BoundValue("f -> n => n * 2").Should().BeOfType<LambdaExpression>().Subject;

        lambda.Parameters.Should().Equal("n");
        lambda.Body.Should().ContainSingle()
            .Which.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void Two_Parameter_Lambda_Parses()
    {
        var lambda = BoundValue("f -> a b => a + b").Should().BeOfType<LambdaExpression>().Subject;

        lambda.Parameters.Should().Equal("a", "b");
    }

    [Fact]
    public void Parenthesized_Lambda_Is_Accepted_As_A_Call_Argument()
    {
        var call = BoundValue("result -> array.map nums (n => n * 2)")
            .Should().BeOfType<ModuleCallExpression>().Subject;

        call.Arguments.Should().HaveCount(2);
        call.Arguments[1].Should().BeOfType<LambdaExpression>()
            .Which.Parameters.Should().Equal("n");
    }

    [Fact]
    public void Unparenthesized_Lambda_As_A_Call_Argument_Is_A_Parse_Error()
    {
        // Deliberate: a bare lambda after a juxtaposed argument is not reachable through
        // ParseAtomArgs (it never calls GetExpression), mirroring how Minus/Not are excluded
        // from AtomStartTokens so `f -x` stays binary subtraction rather than being misparsed.
        var act = () => Parse("result -> array.map nums n => n * 2");

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Plain_Call_Without_Arrow_Still_Parses_As_A_Call()
    {
        // Regression: the new lambda lookahead must not affect ordinary juxtaposed calls.
        var value = BoundValue("y -> f x");

        value.Should().BeOfType<CallExpression>().Which.Callee.Should().Be("f");
    }

    [Fact]
    public void Duplicate_Lambda_Parameter_Name_Throws()
    {
        var act = () => Parse("f -> n n => n");

        act.Should().Throw<Exception>().WithMessage("*already declared*");
    }
}
