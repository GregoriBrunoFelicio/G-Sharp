using FluentAssertions;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class ArrayBuiltinExecutionTests
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
    public void Contains_Finds_A_Present_Element()
    {
        Run("println (array.contains [1 2 3] 2)").Should().Be("True");
    }

    [Fact]
    public void Contains_Reports_Missing_Element()
    {
        Run("println (array.contains [1 2 3] 9)").Should().Be("False");
    }

    [Fact]
    public void IndexOf_Finds_The_Position()
    {
        Run("println (array.indexOf [1 2 3 4 5] 4)").Should().Be("3");
    }

    [Fact]
    public void IndexOf_Returns_Minus_One_When_Missing()
    {
        Run("println (array.indexOf [1 2 3] 9)").Should().Be("-1");
    }

    [Fact]
    public void Slice_Extracts_A_Subrange()
    {
        var source =
            "sliced -> array.slice [1 2 3 4 5] 1 3\n" +
            "println (array.len sliced)\n" +
            "println (array.head sliced)";

        Run(source).Should().Be("2\n2");
    }

    [Fact]
    public void Range_Produces_Consecutive_Ints()
    {
        var source =
            "r -> array.range 0 5\n" +
            "for x in r do\n" +
            "    println x";

        Run(source).Should().Be("0\n1\n2\n3\n4");
    }

    [Fact]
    public void Any_Is_True_When_A_Predicate_Matches()
    {
        Run("println (array.any [1 2 3 4 5] (n => n > 4))").Should().Be("True");
    }

    [Fact]
    public void Any_Is_False_When_No_Predicate_Matches()
    {
        Run("println (array.any [1 2 3 4 5] (n => n > 10))").Should().Be("False");
    }

    [Fact]
    public void All_Is_True_When_Every_Element_Matches()
    {
        Run("println (array.all [1 2 3 4 5] (n => n > 0))").Should().Be("True");
    }

    [Fact]
    public void All_Is_False_When_Some_Element_Fails()
    {
        Run("println (array.all [1 2 3 4 5] (n => n > 2))").Should().Be("False");
    }
}
