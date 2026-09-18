using System.Reflection;
using FluentAssertions;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class IOBuiltinExecutionTests
{
    private static string Run(string source, string stdin = "")
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = ConstantFolder.FoldAll(new GSharp.Compiler.Parser.Parser(tokens).Parse());
        var typeMap = new TypeInferrer().Infer(expressions);

        var originalOut = Console.Out;
        var originalIn = Console.In;
        var captured = new StringWriter();
        Console.SetOut(captured);
        Console.SetIn(new StringReader(stdin));
        try
        {
            new GSharp.Compiler.CodeGen.Compiler().CompileAndRun(expressions, typeMap: typeMap);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
        }

        return captured.ToString().Trim();
    }

    [Fact]
    public void ReadLine_Returns_The_Next_Line_From_Stdin()
    {
        Run("println (io.readLine)", "Ada\n").Should().Be("Ada");
    }

    [Fact]
    public void ReadLine_Returns_Empty_String_At_Eof()
    {
        Run("println (io.readLine)", "").Should().BeEmpty();
    }

    [Fact]
    public void ReadLine_Result_Can_Be_Bound_And_Passed_To_Other_Builtins()
    {
        Run("nome -> io.readLine\nprintln (string.upper nome)", "ada\n").Should().Be("ADA");
    }

    [Fact]
    public void ReadInt_Parses_Stdin_As_An_Int()
    {
        Run("println (io.readInt + 1)", "41\n").Should().Be("42");
    }

    [Fact]
    public void ReadInt_Throws_On_Malformed_Input()
    {
        // CompileAndRun invokes the generated Main via reflection, which wraps any
        // exception thrown by the program in a TargetInvocationException.
        var act = () => Run("println (io.readInt)", "not a number\n");
        act.Should().Throw<TargetInvocationException>()
            .WithInnerException<FormatException>();
    }

    [Fact]
    public void ReadFloat_Parses_Stdin_As_A_Float()
    {
        Run("println (io.readFloat + 0.5f)", "1.5\n").Should().Be("2");
    }
}
