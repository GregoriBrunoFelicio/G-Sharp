using FluentAssertions;
using GSharp.Compiler.AST;

namespace G.Sharp.Compiler.Tests.Parser;

public class CallExpressionParsingTests
{
    private static List<Expression> Parse(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        return new GSharp.Compiler.Parser.Parser(tokens).Parse();
    }

    private static Expression BoundValue(string source) =>
        ((BindingExpression)Parse(source)[0]).Value;

    [Fact]
    public void Parenthesized_First_Argument_Still_Collects_Later_Juxtaposed_Arguments()
    {
        // Regression for the "paren on first arg corrupts codegen" bug: a call whose first
        // argument is parenthesized must keep collecting the remaining juxtaposed arguments
        // instead of treating the parenthesized group as the entire argument list.
        var call = BoundValue("result -> combine (1) 2 3")
            .Should().BeOfType<CallExpression>().Subject;

        call.Arguments.Should().HaveCount(3);
        call.Arguments[0].Should().BeOfType<LiteralExpression>();
        call.Arguments[1].Should().BeOfType<LiteralExpression>();
        call.Arguments[2].Should().BeOfType<LiteralExpression>();
    }

    [Fact]
    public void Parenthesized_Lambda_As_First_Argument_Still_Collects_Later_Arguments()
    {
        var call = BoundValue("result -> apply (n => n * 2) 5")
            .Should().BeOfType<CallExpression>().Subject;

        call.Arguments.Should().HaveCount(2);
        call.Arguments[0].Should().BeOfType<LambdaExpression>();
        call.Arguments[1].Should().BeOfType<LiteralExpression>();
    }

    [Fact]
    public void Parenthesized_Middle_Argument_Still_Parses_As_Before()
    {
        var call = BoundValue("result -> combine 1 (2) 3")
            .Should().BeOfType<CallExpression>().Subject;

        call.Arguments.Should().HaveCount(3);
    }

    [Fact]
    public void Fully_Wrapped_Paren_Call_Still_Parses_As_Before()
    {
        var call = BoundValue("result -> add(3 5)")
            .Should().BeOfType<CallExpression>().Subject;

        call.Arguments.Should().HaveCount(2);
    }

    [Fact]
    public void Single_Argument_Call_With_Parenthesized_Argument_Still_Parses()
    {
        var call = BoundValue("result -> double(5)")
            .Should().BeOfType<CallExpression>().Subject;

        call.Arguments.Should().HaveCount(1);
    }

    [Fact]
    public void Module_Call_With_Parenthesized_First_Argument_Still_Collects_Later_Arguments()
    {
        var call = BoundValue("result -> mod.combine (1) 2 3")
            .Should().BeOfType<ModuleCallExpression>().Subject;

        call.Arguments.Should().HaveCount(3);
    }
}
