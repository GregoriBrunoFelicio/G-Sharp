using FluentAssertions;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class PartialApplicationExecutionTests
{
    private static string Run(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
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
    public void Partial_Application_Of_Named_Function_Yields_Callable_Function_Value()
    {
        var source =
            "add a b => a + b\n" +
            "fn -> add 5\n" +
            "println fn 10";

        Run(source).Should().Be("15");
    }

    [Fact]
    public void Partial_Application_Passed_As_Higher_Order_Argument_Works()
    {
        var source =
            "apply f x => f x\n" +
            "double n => n * 2\n" +
            "fn -> apply double\n" +
            "println fn 5";

        Run(source).Should().Be("10");
    }

    [Fact]
    public void Partial_Application_With_Parenthesized_Lambda_Argument_Works()
    {
        var source =
            "apply f x => f x\n" +
            "fn -> apply (n => n * 2)\n" +
            "println fn 5";

        Run(source).Should().Be("10");
    }

    [Fact]
    public void Chained_Single_Argument_Currying_Works()
    {
        var source =
            "combine a b c => a + b + c\n" +
            "step1 -> combine 1\n" +
            "step2 -> step1 2\n" +
            "println step2 3";

        Run(source).Should().Be("6");
    }

    [Fact]
    public void Partial_Application_Called_With_Remaining_Arguments_At_Once_Works()
    {
        var source =
            "combine a b c => a + b + c\n" +
            "partial -> combine 1\n" +
            "println partial 2 3";

        Run(source).Should().Be("6");
    }

    [Fact]
    public void Unused_Partial_Application_Does_Not_Throw()
    {
        var source =
            "apply f x => f x\n" +
            "fn -> apply (n => n * 2)\n" +
            "println \"ok\"";

        var act = () => Run(source);

        act.Should().NotThrow();
    }
}
