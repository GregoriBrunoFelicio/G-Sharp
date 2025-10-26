using FluentAssertions;
using GSharp.AST;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class CompilerTests
{
    [Fact]
    public void Should_Compile_And_Run_LetStatement()
    {
        var let = new LetStatement(
            "name",
            new LiteralExpression(new StringValue("Greg"))
        );

        var print = new PrintStatement(
            new VariableExpression("name")
        );

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var compiler = new GSharp.CodeGen.Compiler();
        compiler.CompileAndRun([ let, print ]);

        var output = sw.ToString().Trim();
        output.Should().Be("Greg");
    }
    
    [Fact]
    public void Should_Compile_And_Run_AssignmentStatement()
    {
        var let = new LetStatement(
            "x",
            new LiteralExpression(new IntValue(10))
        );

        var assign = new AssignmentStatement(
            "x",
            new LiteralExpression(new IntValue(99))
        );

        var print = new PrintStatement(
            new VariableExpression("x")
        );

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var compiler = new GSharp.CodeGen.Compiler();
        compiler.CompileAndRun([ let, assign, print ]);

        var output = sw.ToString().Trim();
        output.Should().Be("99");
    }
    
    [Fact]
    public void Should_Compile_And_Run_PrintStatement()
    {
        var print = new PrintStatement(
            new LiteralExpression(new StringValue("Hello"))
        );

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var compiler = new GSharp.CodeGen.Compiler();
        compiler.CompileAndRun([ print ]);

        var output = sw.ToString().Trim();
        output.Should().Be("Hello");
    }

    [Fact]
    public void Should_Compile_And_Run_ForStatement()
    {
        var arrayValue = new ArrayValue(
            [
                new StringValue("one"),
                new StringValue("two"),
                new StringValue("three")
            ],
            new GStringType()
        );

        var loop = new ForStatement(
            "item",
            new LiteralExpression(arrayValue),
            [new PrintStatement(new VariableExpression("item"))]
        );

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var compiler = new GSharp.CodeGen.Compiler();
        compiler.CompileAndRun([ loop ]);

        var output = sw.ToString().Trim().Split(Environment.NewLine);
        output.Should().BeEquivalentTo("one", "two", "three");
    }
}