using FluentAssertions;
using GSharp.Compiler.AST;
using GSharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Parser;

public class UnaryExpressionParsingTests
{
    private static Expression BoundValue(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        return ((BindingExpression)expressions[0]).Value;
    }

    [Fact]
    public void Unary_Minus_Binds_Tighter_Than_Binary_Plus()
    {
        var value = BoundValue("y -> -a + b");

        var binary = value.Should().BeOfType<BinaryExpression>().Subject;
        binary.Operator.Should().Be(TokenType.Plus);
        binary.Left.Should().BeOfType<UnaryExpression>()
            .Which.Operator.Should().Be(TokenType.Minus);
        binary.Right.Should().BeOfType<IdentifierExpression>();
    }

    [Fact]
    public void Not_Binds_Tighter_Than_And()
    {
        var value = BoundValue("y -> not a and b");

        var binary = value.Should().BeOfType<BinaryExpression>().Subject;
        binary.Operator.Should().Be(TokenType.And);
        binary.Left.Should().BeOfType<UnaryExpression>()
            .Which.Operator.Should().Be(TokenType.Not);
    }

    [Fact]
    public void Double_Unary_Minus_Chains()
    {
        var value = BoundValue("y -> - -x");

        var outer = value.Should().BeOfType<UnaryExpression>().Subject;
        outer.Operator.Should().Be(TokenType.Minus);
        outer.Operand.Should().BeOfType<UnaryExpression>()
            .Which.Operator.Should().Be(TokenType.Minus);
    }

    [Fact]
    public void Double_Not_Chains()
    {
        var value = BoundValue("y -> not not x");

        var outer = value.Should().BeOfType<UnaryExpression>().Subject;
        outer.Operator.Should().Be(TokenType.Not);
        outer.Operand.Should().BeOfType<UnaryExpression>()
            .Which.Operator.Should().Be(TokenType.Not);
    }

    [Fact]
    public void Not_Symbol_And_Keyword_Produce_The_Same_Node()
    {
        BoundValue("y -> !x").Should().BeOfType<UnaryExpression>()
            .Which.Operator.Should().Be(TokenType.Not);
        BoundValue("y -> not x").Should().BeOfType<UnaryExpression>()
            .Which.Operator.Should().Be(TokenType.Not);
    }

    [Fact]
    public void Unary_Minus_Wraps_Function_Application_Not_The_Reverse()
    {
        var value = BoundValue("y -> -f x");

        var unary = value.Should().BeOfType<UnaryExpression>().Subject;
        unary.Operand.Should().BeOfType<CallExpression>()
            .Which.Callee.Should().Be("f");
    }

    [Fact]
    public void Juxtaposed_Minus_After_An_Identifier_Still_Means_Binary_Subtraction()
    {
        // Backward-compatibility guarantee: `f -x` is not reinterpreted as "call f with -x".
        // Minus/Not were deliberately not added to AtomStartTokens.
        var value = BoundValue("y -> f -x");

        var binary = value.Should().BeOfType<BinaryExpression>().Subject;
        binary.Operator.Should().Be(TokenType.Minus);
        binary.Left.Should().BeOfType<IdentifierExpression>().Which.Name.Should().Be("f");
        binary.Right.Should().BeOfType<IdentifierExpression>().Which.Name.Should().Be("x");
    }
}
