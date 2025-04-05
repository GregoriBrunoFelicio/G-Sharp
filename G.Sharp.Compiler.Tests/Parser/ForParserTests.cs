using FluentAssertions;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.Tests.Parser;

public class ForParserTests
{
    [Fact]
    public void Parse_For_With_Variable_Iterable_Should_Return_Valid_For_Statement()
    {
        var code = "for item in nums { println item; }";
        var tokens = new Compiler.Lexer.Lexer(code).Tokenize();
        var parser = new Parsers.Parser(tokens);
        var statements = parser.Parse();

        statements.Should().HaveCount(1);
        statements[0].Should().BeOfType<ForStatement>();

        var forStatement = (statements[0] as ForStatement)!;

        forStatement.Variable.Should().Be("item");
        forStatement.Iterable.Should().BeOfType<VariableExpression>();
        ((VariableExpression)forStatement.Iterable).Name.Should().Be("nums");

        forStatement.Body.Should().HaveCount(1);
        forStatement.Body[0].Should().BeOfType<PrintStatement>();
        ((PrintStatement)forStatement.Body[0]).VariableName.Should().Be("item");
    }

    [Fact]
    public void Parse_For_With_Array_Literal_Should_Return_Literal_As_Iterable()
    {
        var code = "for n in [1 2 3] { println n; }";
        var tokens = new Compiler.Lexer.Lexer(code).Tokenize();
        var parser = new Parsers.Parser(tokens);
        var statements = parser.Parse();

        statements.Should().HaveCount(1);
        statements[0].Should().BeOfType<ForStatement>();

        var forStatement = (statements[0] as ForStatement)!;
        forStatement.Variable.Should().Be("n");
        forStatement.Iterable.Should().BeOfType<LiteralExpression>();
        forStatement.Body.Should().HaveCount(1);
        forStatement.Body[0].Should().BeOfType<PrintStatement>();
    }

    [Fact]
    public void Parse_For_With_Empty_Body_Should_Return_Empty_Body_List()
    {
        var code = "for x in xs { }";
        var tokens = new Compiler.Lexer.Lexer(code).Tokenize();
        var parser = new Parsers.Parser(tokens);
        var statements = parser.Parse();

        statements.Should().HaveCount(1);
        var forStatement = (statements[0] as ForStatement)!;
        forStatement.Body.Should().BeEmpty();
    }
}