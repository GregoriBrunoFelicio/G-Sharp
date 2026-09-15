using FluentAssertions;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class MatchExecutionTests
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
    public void Matches_An_Int_Scrutinee_Against_Literal_Arms()
    {
        var source =
            "describe n =>\n" +
            "    match n\n" +
            "        1 => \"one\"\n" +
            "        2 => \"two\"\n" +
            "        n => \"other\"\n" +
            "println (describe 1)\n" +
            "println (describe 2)\n" +
            "println (describe 5)";

        Run(source).Should().Be("one\ntwo\nother");
    }

    [Fact]
    public void Matches_A_String_Scrutinee_Against_Literal_Arms()
    {
        var source =
            "greet name =>\n" +
            "    match name\n" +
            "        \"Alice\" => \"hi Alice\"\n" +
            "        n => \"hi stranger\"\n" +
            "println (greet \"Alice\")\n" +
            "println (greet \"Bob\")";

        Run(source).Should().Be("hi Alice\nhi stranger");
    }

    [Fact]
    public void Matches_A_Bool_Scrutinee_Against_Literal_Arms()
    {
        var source =
            "describe flag =>\n" +
            "    match flag\n" +
            "        true => \"yes\"\n" +
            "        n => \"no\"\n" +
            "println (describe true)\n" +
            "println (describe false)";

        Run(source).Should().Be("yes\nno");
    }

    [Fact]
    public void Catch_All_Binds_And_Uses_The_Scrutinee_Value()
    {
        var source =
            "x -> match 5\n" +
            "    1 => \"one\"\n" +
            "    n => \"other: \" + string.from n\n" +
            "println x";

        Run(source).Should().Be("other: 5");
    }

    [Fact]
    public void Nested_Match_Works()
    {
        var source =
            "x -> match 1\n" +
            "    1 => match 2\n" +
            "        2 => \"inner two\"\n" +
            "        m => \"inner other\"\n" +
            "    n => \"outer other\"\n" +
            "println x";

        Run(source).Should().Be("inner two");
    }

    [Fact]
    public void Lambda_Inside_An_Arm_Body_Executes_Correctly()
    {
        // The fallback arm deliberately avoids a bare array literal (`[0]`) as a single-line arm
        // body: that hits a pre-existing, match-unrelated gap where ParseNext() (used for a
        // same-line body via ParseScopedFunctionBody) has no case for a leading '[' token — the
        // same gap already affects `if`/`for`'s single-line bodies today, not something this
        // feature introduces.
        var source =
            "x -> match 1\n" +
            "    1 => array.map [1 2 3] (m => m * 2)\n" +
            "    n => array.range 0 0\n" +
            "for v in x do\n" +
            "    println v";

        Run(source).Should().Be("2\n4\n6");
    }

    [Fact]
    public void Recursive_Function_Dispatched_Via_Match_Computes_Factorial()
    {
        var source =
            "factorial n =>\n" +
            "    match n\n" +
            "        0 => 1\n" +
            "        n => n * factorial (n - 1)\n" +
            "println (factorial 5)";

        Run(source).Should().Be("120");
    }

    [Fact]
    public void Catch_All_Name_Does_Not_Leak_Into_Code_After_The_Match()
    {
        var source =
            "h -> 7\n" +
            "inner -> match 99\n" +
            "    1 => \"one\"\n" +
            "    h => \"shadowed h = \" + string.from h\n" +
            "println inner\n" +
            "println h";

        // Proves the save/restore of EmitContext.Locals around the catch-all arm works: the
        // arm's own binding of "h" (= 99) must not leak into the outer "h" (= 7) afterward —
        // the same class of bug found for `for`'s loop-variable shadowing during this feature's
        // design investigation.
        Run(source).Should().Be("shadowed h = 99\n7");
    }
}
