using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;

namespace G.Sharp.Compiler.Tests.Parser;

public class AssignmentParserTests
{
    [Fact]
    public void Should_Parse_Assignment_With_String()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "name"),
            new(TokenType.Equals, "="),
            new(TokenType.StringLiteral, "Greg"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        parser.VariablesDeclared.Add("name", GType.String);

        var result = new AssignmentParser(parser).Parse();

        var statement = result.As<AssignmentStatement>();
        statement.VariableName.Should().Be("name");
        statement.VariableValue.Should().BeOfType<StringValue>();
        statement.VariableValue.As<StringValue>().Value.Should().Be("Greg");
    }
    
    [Fact]
    public void Should_Parse_Assignment_With_Int()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "x"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "42"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        parser.VariablesDeclared.Add("x", GType.Number);

        var result = new AssignmentParser(parser).Parse();

        var statement = result.As<AssignmentStatement>();
        statement.VariableName.Should().Be("x");
        statement.VariableValue.Should().BeOfType<IntValue>();
        statement.VariableValue.As<IntValue>().Value.Should().Be(42);
    }
    
    [Fact]
    public void Should_Parse_Assignment_With_Boolean_True()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "flag"),
            new(TokenType.Equals, "="),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        parser.VariablesDeclared.Add("flag", GType.Boolean);

        var result = new AssignmentParser(parser).Parse();

        var statement = result.As<AssignmentStatement>();
        statement.VariableName.Should().Be("flag");
        statement.VariableValue.Should().BeOfType<BooleanValue>();
        statement.VariableValue.As<BooleanValue>().Value.Should().BeTrue();
    }
    
    [Fact]
    public void Should_Parse_Assignment_With_Boolean_False()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "flag"),
            new(TokenType.Equals, "="),
            new(TokenType.BooleanFalseLiteral, "false"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        parser.VariablesDeclared.Add("flag", GType.Boolean);

        var result = new AssignmentParser(parser).Parse();

        var statement = result.As<AssignmentStatement>();
        statement.VariableName.Should().Be("flag");
        statement.VariableValue.Should().BeOfType<BooleanValue>();
        statement.VariableValue.As<BooleanValue>().Value.Should().BeFalse();
    }
    
    [Fact]
    public void Should_Assign_Value_To_Already_Declared_Variable()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "x"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "100"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        parser.VariablesDeclared.Add("x", GType.Number);

        var result = new AssignmentParser(parser).Parse();

        var stmt = result.As<AssignmentStatement>();
        stmt.VariableName.Should().Be("x");
        stmt.VariableValue.Should().BeOfType<IntValue>();
        stmt.VariableValue.As<IntValue>().Value.Should().Be(100);
    }
}