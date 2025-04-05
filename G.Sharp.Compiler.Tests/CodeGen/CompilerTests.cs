using FluentAssertions;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class CompilerTests
{
    [Fact]
    public void CompileAndRun_Should_Print_Let_Variable()
    {
        var statements = new List<Statement>
        {
            new LetStatement("x", new IntValue(42)),
            new PrintStatement("x")
        };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var compiler = new Compiler.CodeGen.Compiler();
        compiler.CompileAndRun(statements);

        sw.ToString().Trim().Should().Be("42");
    }

    [Fact]
    public void CompileAndRun_Should_Print_Assigned_Value()
    {
        var statements = new List<Statement>
        {
            new LetStatement("x", new IntValue(0)),
            new AssignmentStatement("x", new IntValue(99)),
            new PrintStatement("x")
        };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var compiler = new Compiler.CodeGen.Compiler();
        compiler.CompileAndRun(statements);

        sw.ToString().Trim().Should().Be("99");
    }

    [Fact]
    public void CompileAndRun_Should_Throw_When_Statement_Is_Not_Supported()
    {
        var invalidStatement = new FakeStatement();
        var statements = new List<Statement> { invalidStatement };
        var compiler = new Compiler.CodeGen.Compiler();

        var act = () => compiler.CompileAndRun(statements);

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Unsupported statement: FakeStatement");
    }

    private record FakeStatement : Statement { }
}