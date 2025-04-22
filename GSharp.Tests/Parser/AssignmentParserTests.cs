using FluentAssertions;
using GSharp.AST;
using GSharp.Lexer;
using GSharp.Parser;

namespace G.Sharp.Compiler.Tests.Parser;

public class AssignmentParserTests
{
     [Fact]
    public void Should_Parse_Assignment_With_IntValue()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "x"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "42"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens)
        {
            VariablesDeclared =
            {
                ["x"] = new GType(GPrimitiveType.Int)
            }
        };

        var result = new AssignmentParser(parser).Parse();

        result.VariableName.Should().Be("x");
        result.Expression.Should().BeOfType<LiteralExpression>();
        result.Expression.As<LiteralExpression>().Value.Should().BeOfType<IntValue>();
        result.Expression.As<LiteralExpression>().Value.As<IntValue>().Value.Should().Be(42);
    }

    [Fact]
    public void Should_Throw_If_Variable_Is_Not_Declared()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "y"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "10"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);

        var act = () => new AssignmentParser(parser).Parse();

        act.Should().Throw<Exception>()
            .WithMessage("Variable 'y' is not declared.");
    }

    [Fact]
    public void Should_Throw_If_Type_Is_Not_Compatible()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "x"),
            new(TokenType.Equals, "="),
            new(TokenType.StringLiteral, "hello"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens)
        {
            VariablesDeclared =
            {
                ["x"] = new GType(GPrimitiveType.Int)
            }
        };

        var act = () => new AssignmentParser(parser).Parse();

        act.Should().Throw<Exception>()
            .WithMessage("Type mismatch: expected Int, but got String");
    }
}