using FluentAssertions;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class LambdaExecutionTests
{
    // Mirrors the real pipeline (GSharp.CLI/Program.cs): fold before type-checking, since
    // LambdaLifter operates on the post-fold tree and must see the same node references
    // TypeInferrer's TypeMap was built against.
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
    public void Lambda_Bound_To_A_Name_And_Called_Directly()
    {
        var source =
            "f -> n => n * 2\n" +
            "result -> f 3\n" +
            "println result";

        Run(source).Should().Be("6");
    }

    [Fact]
    public void Lambda_Passed_Inline_To_An_Existing_Higher_Order_Function()
    {
        var source =
            "apply x f => f x\n" +
            "result -> apply 5 (n => n * 2)\n" +
            "println result";

        Run(source).Should().Be("10");
    }

    [Fact]
    public void Lambda_Passed_Inline_To_Array_Map()
    {
        var source =
            "doubled -> array.map [1 2 3] (n => n * 2)\n" +
            "println (array.fold doubled 0 (acc e => acc + e))";

        Run(source).Should().Be("12");
    }

    [Fact]
    public void Lambda_Passed_Inline_To_Array_Filter()
    {
        var source = "println (array.len (array.filter [1 2 3 4 5] (n => n > 2)))";

        Run(source).Should().Be("3");
    }

    [Fact]
    public void Lambda_Passed_Inline_To_Array_Fold()
    {
        var source = "println (array.fold [1 2 3 4] 0 (acc e => acc + e))";

        Run(source).Should().Be("10");
    }

    [Fact]
    public void Nested_Non_Capturing_Lambdas_Both_Lift_Independently()
    {
        var source =
            "apply x f => f x\n" +
            "outer -> n => apply n (m => m * 2)\n" +
            "result -> outer 5\n" +
            "println result";

        Run(source).Should().Be("10");
    }

    [Fact]
    public void Capturing_Nested_Lambda_Throws_At_Codegen()
    {
        // Non-capturing lambdas are lifted to independent top-level functions, so a lambda
        // referencing an *enclosing* lambda's parameter type-checks fine (the type checker's
        // scope chaining has no notion of "lambda boundary") but fails at codegen — the lifted
        // inner function has no visibility into the outer lambda's parameter. This locks in
        // today's behavior (a clean throw, not silently wrong codegen) as a known, deliberately
        // out-of-scope rough edge — see GSharp.Compiler/CodeGen/LambdaLifter.cs.
        var source =
            "apply f => f 5\n" +
            "outer -> a => apply (b => a + b)\n" +
            "result -> outer 1\n" +
            "println result";

        var act = () => Run(source);

        act.Should().Throw<Exception>();
    }
}
