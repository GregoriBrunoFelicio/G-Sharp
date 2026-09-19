using System.Globalization;
using FluentAssertions;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class TimeBuiltinExecutionTests
{
    private static string Run(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = ConstantFolder.FoldAll(new GSharp.Compiler.Parser.Parser(tokens).Parse());
        var typeMap = new TypeInferrer().Infer(expressions);

        var originalOut = Console.Out;
        var captured = new StringWriter();
        Console.SetOut(captured);
        try
        {
            new GSharp.Compiler.CodeGen.Compiler().CompileAndRun(expressions, typeMap: typeMap);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return captured.ToString().Trim();
    }

    [Fact]
    public void Now_Prints_Like_CSharp_DateTime()
    {
        var before = DateTime.Now;
        var printed = DateTime.Parse(Run("println (time.now)"), CultureInfo.CurrentCulture);
        var after = DateTime.Now;

        printed.Should().BeOnOrAfter(before.AddSeconds(-1)).And.BeOnOrBefore(after.AddSeconds(1));
    }

    [Fact]
    public void Make_Prints_With_DateTime_ToString()
    {
        Run("println (time.make 2026 9 19)").Should().Be(new DateTime(2026, 9, 19).ToString());
    }

    [Fact]
    public void Format_Uses_A_Custom_Pattern()
    {
        Run("println (time.format (time.make 2026 9 19) \"yyyy-MM-dd\")").Should().Be("2026-09-19");
    }

    [Fact]
    public void Components_Are_Extracted()
    {
        Run("d -> time.make 2026 9 19\nprintln (time.year d)\nprintln (time.month d)\nprintln (time.day d)")
            .Replace("\r", "").Should().Be("2026\n9\n19");
    }

    [Fact]
    public void Parse_Then_Format_Round_Trips()
    {
        Run("println (time.format (time.parse \"2026-09-19\") \"yyyy-MM-dd\")").Should().Be("2026-09-19");
    }

    [Fact]
    public void AddDays_Crosses_A_Month_Boundary()
    {
        Run("println (time.format (time.addDays (time.parse \"2026-09-19\") 30) \"yyyy-MM-dd\")")
            .Should().Be("2026-10-19");
    }

    [Fact]
    public void AddHours_Advances_The_Clock()
    {
        Run("println (time.hour (time.addHours (time.make 2026 1 1) 5))").Should().Be("5");
    }

    [Fact]
    public void Weekday_Of_A_Known_Saturday()
    {
        Run("println (time.weekday (time.make 2026 9 19))").Should().Be("6");
    }

    [Fact]
    public void Dates_Compare_With_Ordinary_Operators()
    {
        Run("println ((time.make 2026 1 1) < (time.make 2026 6 1))").Should().Be("True");
        Run("println ((time.make 2026 1 1) == (time.make 2026 1 1))").Should().Be("True");
        Run("println ((time.make 2026 1 1) >= (time.make 2026 6 1))").Should().Be("False");
    }

    [Fact]
    public void Diff_Counts_Whole_Units()
    {
        Run("println (time.diffDays (time.make 2026 9 19) (time.make 2026 9 1))").Should().Be("18");
        Run("println (time.diffSeconds (time.make 2026 1 1) (time.make 2026 1 2))").Should().Be("-86400");
    }

    [Fact]
    public void Date_And_Int_Do_Not_Unify()
    {
        var act = () => Run("println ((time.make 2026 1 1) < 5)");
        act.Should().Throw<Exception>().WithMessage("*type mismatch*");
    }
}
