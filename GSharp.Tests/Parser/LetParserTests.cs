using FluentAssertions;
using GSharp.AST;
using GSharp.Lexer;
using GSharp.Parser;

namespace G.Sharp.Compiler.Tests.Parser;

public class LetParserTests
{
    [Fact]
    public void Should_Throw_If_Variable_Name_Is_Already_Declared()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens)
        {
            VariablesDeclared =
            {
                ["x"] = new GNumberType()
            }
        };

        var act = () => new LetParser(parser).Parse();

        act.Should().Throw<Exception>()
            .WithMessage("Variable x already declared.");
    }

    [Fact]
    public void Should_Throw_If_Variable_Name_Is_Invalid()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "1invalid"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);

        var act = () => new LetParser(parser).Parse();

        act.Should().Throw<Exception>()
            .WithMessage("Invalid variable name: 1invalid");
    }

    [Fact]
    public void Should_Throw_If_Variable_Name_Is_Reserved()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "let"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);

        var act = () => new LetParser(parser).Parse();

        act.Should().Throw<Exception>()
            .WithMessage("'let' is a reserved keyword.");
    }

    [Fact]
    public void Should_Throw_If_Type_Is_Not_Compatible()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.Equals, "="),
            new(TokenType.StringLiteral, "\"abc\""),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);

        var act = () => new LetParser(parser).Parse();

        act.Should().Throw<Exception>()
            .WithMessage("Type mismatch: expected Number, but got String");
    }

    [Fact]
    public void Should_Return_LetStatement_If_All_Is_Valid()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "42"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var letParser = new LetParser(parser);

        var result = letParser.Parse();

        result.VariableName.Should().Be("x");
        result.Expression.Should().BeOfType<LiteralExpression>();

        var value = result.Expression.GetLiteralValue();
        value.Should().BeOfType<IntValue>();
        value.As<IntValue>().Value.Should().Be(42);
        // value.Type.Should().Be(new GNumberType());
    }
}
