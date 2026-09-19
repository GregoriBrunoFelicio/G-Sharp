using FluentAssertions;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class MapIterationExecutionTests
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

        return captured.ToString().Replace("\r", "").Trim();
    }

    private const string Scores = "scores -> {\"ana\": 90 \"bob\": 85}\n";

    [Fact]
    public void For_Over_A_Map_Exposes_Key_And_Value()
    {
        Run(Scores + "for e in scores do\n    println e.key\n    println e.value\n")
            .Should().Be("ana\n90\nbob\n85");
    }

    [Fact]
    public void For_Over_A_Map_Returns_An_Array_Of_Body_Results()
    {
        Run(Scores + "totals -> for e in scores do\n    e.value + 1\nprintln totals").Should().Be("[91, 86]");
    }

    [Fact]
    public void Printing_An_Entry_Shows_Key_And_Value()
    {
        Run(Scores + "for e in scores do\n    println e").Should().Be("ana: 90\nbob: 85");
    }

    [Fact]
    public void Entries_Work_With_Array_Map()
    {
        Run(Scores + "println (array.map (map.entries scores) (e => e.key))").Should().Be("[ana, bob]");
    }

    [Fact]
    public void Entries_Work_With_Array_Fold()
    {
        Run(Scores + "println (array.fold (map.entries scores) 0 (acc e => acc + e.value))").Should().Be("175");
    }

    [Fact]
    public void GetOr_Returns_The_Value_Or_The_Fallback()
    {
        Run(Scores + "println (map.getOr scores \"ana\" 0)\nprintln (scores.getOr \"zed\" 0)")
            .Should().Be("90\n0");
    }

    [Fact]
    public void Word_Count_With_Fold_And_GetOr()
    {
        Run("words -> [\"a\" \"b\" \"a\"]\n" +
            "counts -> array.fold words (map.empty) (acc w => map.set acc w (map.getOr acc w 0 + 1))\n" +
            "println counts").Should().Be("{a: 2, b: 1}");
    }

    [Fact]
    public void For_Over_An_Empty_Map_Runs_Zero_Times()
    {
        Run("m -> map.empty\nresult -> for e in m do\n    e.key\nprintln result").Should().Be("[]");
    }

    [Fact]
    public void Entry_Holding_An_Array_Value_Prints_Recursively()
    {
        Run("m -> {\"a\": [1 2]}\nfor e in m do\n    println e").Should().Be("a: [1, 2]");
    }

    [Fact]
    public void For_Over_An_Array_And_Over_Keys_Still_Work()
    {
        Run("for n in [1 2] do\n    println n").Should().Be("1\n2");
        Run(Scores + "for k in scores.keys do\n    println k").Should().Be("ana\nbob");
    }
}
