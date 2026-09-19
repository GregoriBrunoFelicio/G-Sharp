using FluentAssertions;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class MapBuiltinExecutionTests
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
    public void Map_Literal_Pretty_Prints_As_Key_Colon_Value_Pairs()
    {
        Run("println {\"a\": 1 \"b\": 2}").Should().Be("{a: 1, b: 2}");
    }

    [Fact]
    public void Empty_Map_Literal_Pretty_Prints_As_Empty_Braces()
    {
        Run("println {}").Should().Be("{}");
    }

    [Fact]
    public void Map_Values_Can_Be_Computed_Expressions_Or_Bindings()
    {
        Run("x -> 10\nprintln {\"sum\": 1 + 1 \"x\": x}").Should().Be("{sum: 2, x: 10}");
    }

    [Fact]
    public void Map_Set_Never_Mutates_The_Original_Map()
    {
        Run("m -> {\"a\": 1}\nm2 -> map.set m \"b\" 2\nprintln m\nprintln m2")
            .Should().Be("{a: 1}\n{a: 1, b: 2}");
    }

    [Fact]
    public void Map_Get_Returns_The_Value_For_A_Key()
    {
        Run("m -> {\"a\": 1 \"b\": 2}\nprintln (map.get m \"b\")").Should().Be("2");
    }

    [Fact]
    public void Map_Has_Checks_Key_Presence()
    {
        Run("m -> {\"a\": 1}\nprintln (map.has m \"a\")\nprintln (map.has m \"z\")")
            .Should().Be("True\nFalse");
    }

    [Fact]
    public void Map_Remove_Returns_A_New_Map_Without_The_Key()
    {
        Run("m -> {\"a\": 1 \"b\": 2}\nprintln (map.remove m \"a\")").Should().Be("{b: 2}");
    }

    [Fact]
    public void Map_Keys_And_Values_Return_Arrays()
    {
        Run("m -> {\"a\": 1 \"b\": 2}\nprintln (map.keys m)\nprintln (map.values m)")
            .Should().Be("[a, b]\n[1, 2]");
    }

    [Fact]
    public void Map_Size_Returns_The_Entry_Count()
    {
        Run("m -> {\"a\": 1 \"b\": 2}\nprintln (map.size m)").Should().Be("2");
    }

    [Fact]
    public void Map_Empty_Builds_An_Empty_Map()
    {
        Run("println map.empty").Should().Be("{}");
    }
}
