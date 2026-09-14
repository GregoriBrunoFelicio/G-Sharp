using FluentAssertions;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class PrintExecutionTests
{
    private static string Run(string source)
    {
        return RunRaw(source).Trim();
    }

    private static string RunRaw(string source)
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

        return captured.ToString();
    }

    // -------------------------------------------------------------------------
    // print vs println newline behavior
    // -------------------------------------------------------------------------

    [Fact]
    public void Print_Does_Not_Add_A_Trailing_Newline()
    {
        RunRaw("print 1").Should().Be("1");
    }

    [Fact]
    public void Println_Still_Adds_A_Trailing_Newline()
    {
        RunRaw("println 1").Should().Be("1" + Environment.NewLine);
    }

    [Fact]
    public void Consecutive_Prints_Have_No_Separator_Between_Them()
    {
        RunRaw("print 1\nprint 2").Should().Be("12");
    }

    // -------------------------------------------------------------------------
    // Array pretty-printing (print/println only)
    // -------------------------------------------------------------------------

    [Fact]
    public void Prints_A_Flat_Array_As_A_Bracketed_Comma_Joined_List()
    {
        Run("println [1 2 3]").Should().Be("[1, 2, 3]");
    }

    [Fact]
    public void Prints_A_Nested_Array_Recursively()
    {
        // Nested array *literals* can't be constructed yet (EmitArrayElement has no case for
        // an object[] element — a pre-existing, separate gap in array-literal codegen), so this
        // builds the nested array at runtime via array.map instead, to exercise
        // FormatForDisplay's recursion on a genuinely nested object[].
        Run("println array.map [1 2] (n => array.range 0 n)").Should().Be("[[0], [0, 1]]");
    }

    [Fact]
    public void Prints_An_Empty_Array_As_Empty_Brackets()
    {
        Run("println []").Should().Be("[]");
    }

    [Fact]
    public void Print_Also_Pretty_Prints_Arrays()
    {
        RunRaw("print [1 2]").Should().Be("[1, 2]");
    }

    [Fact]
    public void Scalar_Println_Output_Is_Unchanged_Including_Bool_Capitalization()
    {
        // Locks in the pre-existing C# bool.ToString() capitalization quirk — out of scope to change.
        Run("println true").Should().Be("True");
    }
}
