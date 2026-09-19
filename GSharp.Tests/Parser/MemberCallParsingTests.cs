using FluentAssertions;
using GSharp.Compiler.AST;

namespace G.Sharp.Compiler.Tests.Parser;

public class MemberCallParsingTests
{
    private static Expression BoundValue(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        return ((BindingExpression)new GSharp.Compiler.Parser.Parser(tokens).Parse()[0]).Value;
    }

    [Fact]
    public void Member_After_A_Module_Call_Wraps_It_As_The_Receiver()
    {
        var member = BoundValue("y -> time.now.year").Should().BeOfType<MemberCallExpression>().Subject;

        member.Member.Should().Be("year");
        member.Receiver.Should().BeOfType<ModuleCallExpression>();
    }

    [Fact]
    public void Member_After_A_Parenthesized_Expression_Collects_Arguments()
    {
        var member = BoundValue("d -> (time.make 2026 9 19).addDays 30")
            .Should().BeOfType<MemberCallExpression>().Subject;

        member.Member.Should().Be("addDays");
        member.Arguments.Should().HaveCount(1);
    }

    [Fact]
    public void Members_Chain_Left_To_Right()
    {
        var outer = BoundValue("d -> (x).a.b").Should().BeOfType<MemberCallExpression>().Subject;

        outer.Member.Should().Be("b");
        outer.Receiver.Should().BeOfType<MemberCallExpression>().Which.Member.Should().Be("a");
    }

    [Fact]
    public void Identifier_Dot_Member_Stays_A_Module_Call_With_A_Receiver()
    {
        var call = BoundValue("y -> current.year").Should().BeOfType<ModuleCallExpression>().Subject;

        call.Receiver.Should().BeOfType<IdentifierExpression>().Which.Name.Should().Be("current");
    }

    [Fact]
    public void Unary_Minus_Applies_To_The_Whole_Member_Call()
    {
        var unary = BoundValue("y -> -(x).f").Should().BeOfType<UnaryExpression>().Subject;

        unary.Operand.Should().BeOfType<MemberCallExpression>();
    }
}
