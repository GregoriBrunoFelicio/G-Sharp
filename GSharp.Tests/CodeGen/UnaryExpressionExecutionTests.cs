using FluentAssertions;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class UnaryExpressionExecutionTests
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

        return captured.ToString().Replace("\r\n", "\n").Trim();
    }

    [Fact]
    public void Negates_An_Int()
    {
        Run("x -> 5\nprintln -x").Should().Be("-5");
    }

    [Fact]
    public void Negates_A_Float()
    {
        Run("x -> 5.0f\nprintln -x").Should().Be("-5");
    }

    [Fact]
    public void Negates_A_Double()
    {
        Run("x -> 5.0d\nprintln -x").Should().Be("-5");
    }

    [Fact]
    public void Negates_A_Decimal_Through_The_Boxed_Fallback()
    {
        Run("x -> 5.0m\nprintln -x").Should().Be("-5.0");
    }

    [Fact]
    public void Nots_A_Bool()
    {
        Run("x -> true\nprintln not x").Should().Be("False");
    }

    [Fact]
    public void Negates_A_Parenthesized_Sum()
    {
        Run("println -(1 + 2)").Should().Be("-3");
    }

    [Fact]
    public void Not_On_A_Comparison()
    {
        Run("println not (1 == 1)").Should().Be("False");
    }

    [Fact]
    public void Unary_Minus_Composes_With_Binary_Plus()
    {
        Run("a -> 10\nb -> 3\nprintln -a + b").Should().Be("-7");
    }
}
