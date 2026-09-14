using FluentAssertions;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class StringBuiltinExecutionTests
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
    public void Len_Returns_Character_Count()
    {
        Run("println string.len \"hello\"").Should().Be("5");
    }

    [Fact]
    public void From_On_An_Array_Still_Uses_Raw_ToString_Not_Pretty_Printing()
    {
        Run("println string.from [1 2 3]").Should().Be("System.Object[]");
    }

    [Fact]
    public void Upper_Uppercases_The_String()
    {
        Run("println string.upper \"hello\"").Should().Be("HELLO");
    }

    [Fact]
    public void Lower_Lowercases_The_String()
    {
        Run("println string.lower \"HELLO\"").Should().Be("hello");
    }

    [Fact]
    public void Trim_Removes_Surrounding_Whitespace()
    {
        Run("println string.trim \"  hi  \"").Should().Be("hi");
    }

    [Fact]
    public void Contains_Finds_A_Substring()
    {
        Run("println string.contains \"hello world\" \"world\"").Should().Be("True");
    }

    [Fact]
    public void StartsWith_Checks_The_Prefix()
    {
        Run("println string.startsWith \"hello\" \"he\"").Should().Be("True");
    }

    [Fact]
    public void EndsWith_Checks_The_Suffix()
    {
        Run("println string.endsWith \"hello\" \"lo\"").Should().Be("True");
    }

    [Fact]
    public void Replace_Substitutes_All_Occurrences()
    {
        Run("println (string.replace \"a-b-c\" \"-\" \"_\")").Should().Be("a_b_c");
    }

    [Fact]
    public void Slice_Extracts_A_Substring_By_Index_Range()
    {
        Run("println (string.slice \"hello world\" 0 5)").Should().Be("hello");
    }

    [Fact]
    public void ToInt_Parses_A_Numeric_String()
    {
        Run("println (string.toInt \"42\" + 1)").Should().Be("43");
    }

    [Fact]
    public void ToFloat_Parses_A_Decimal_String()
    {
        Run("println (string.toFloat \"3.5\" + 0.5f)").Should().Be("4");
    }

    [Fact]
    public void Split_Produces_An_Array_Usable_By_Array_Builtins()
    {
        var source =
            "parts -> string.split \"a,b,c\" \",\"\n" +
            "println (array.len parts)\n" +
            "println (array.head parts)";

        Run(source).Should().Be("3\na");
    }

    [Fact]
    public void Join_Rejoins_A_Split_String()
    {
        var source =
            "parts -> string.split \"a,b,c\" \",\"\n" +
            "println (string.join parts \",\")";

        Run(source).Should().Be("a,b,c");
    }
}
