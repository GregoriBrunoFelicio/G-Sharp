using FluentAssertions;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class MathBuiltinExecutionTests
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
    public void Abs_Handles_A_Negative_Int()
    {
        Run("println (math.abs (0 - 5))").Should().Be("5");
    }

    [Fact]
    public void Abs_Handles_A_Negative_Double()
    {
        Run("println (math.abs (0.0d - 5.5d))").Should().Be("5.5");
    }

    [Fact]
    public void Floor_Rounds_Down()
    {
        Run("println math.floor 3.7d").Should().Be("3");
    }

    [Fact]
    public void Ceil_Rounds_Up()
    {
        Run("println math.ceil 3.2d").Should().Be("4");
    }

    [Fact]
    public void Round_Rounds_To_Nearest()
    {
        Run("println math.round 3.4d").Should().Be("3");
    }

    [Fact]
    public void Sqrt_Returns_A_Double()
    {
        Run("println math.sqrt 16").Should().Be("4");
    }

    [Fact]
    public void Pow_Raises_To_An_Exponent()
    {
        Run("println (math.pow 2 10)").Should().Be("1024");
    }

    [Fact]
    public void Min_Returns_The_Smaller_Value()
    {
        Run("println (math.min 3 7)").Should().Be("3");
    }

    [Fact]
    public void Max_Returns_The_Larger_Value()
    {
        Run("println (math.max 3 7)").Should().Be("7");
    }

    [Fact]
    public void Mod_Returns_The_Remainder()
    {
        Run("println (math.mod 10 3)").Should().Be("1");
    }

    [Fact]
    public void Pi_Is_A_Zero_Argument_Call()
    {
        Run("println math.pi").Should().Be("3.141592653589793");
    }

    [Fact]
    public void E_Is_A_Zero_Argument_Call()
    {
        Run("println math.e").Should().Be("2.718281828459045");
    }
}
