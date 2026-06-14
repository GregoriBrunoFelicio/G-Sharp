using FluentAssertions;
using GSharp.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class TailCallOptimizationTests
{
    private static string Run(string source)
    {
        var tokens      = new GSharp.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Parser.Parser(tokens).Parse();
        var typeMap     = new TypeInferrer().Infer(expressions);

        var originalOut = Console.Out;
        var captured    = new StringWriter();
        Console.SetOut(captured);
        try
        {
            new GSharp.CodeGen.Compiler().CompileAndRun(expressions, typeMap: typeMap);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return captured.ToString().Trim();
    }

    [Fact]
    public void Tail_Recursive_Sum_Produces_Correct_Result()
    {
        var source =
            "sum acc n\n" +
            "    if n == 0 then\n" +
            "        acc\n" +
            "    else\n" +
            "        let a = acc + n\n" +
            "        let b = n - 1\n" +
            "        sum a b\n" +
            "let result = sum 0 100\n" +
            "println result";

        Run(source).Should().Be("5050");
    }

    [Fact]
    public void Tail_Recursive_Sum_Does_Not_Overflow_With_Large_Input()
    {
        // Without TCO this depth would cause a StackOverflowException
        var source =
            "sum acc n\n" +
            "    if n == 0 then\n" +
            "        acc\n" +
            "    else\n" +
            "        let a = acc + n\n" +
            "        let b = n - 1\n" +
            "        sum a b\n" +
            "let result = sum 0 100000\n" +
            "println result";

        var act = () => Run(source);

        act.Should().NotThrow();
    }
}
