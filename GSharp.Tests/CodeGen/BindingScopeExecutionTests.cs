using FluentAssertions;
using GSharp.TypeChecker;

namespace G.Sharp.Compiler.Tests.Scoping;

// End-to-end: a name reused inside a function and at the top level compiles and runs with each
// scope keeping its own value.
public class BindingScopeExecutionTests
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

        return captured.ToString();
    }

    [Fact]
    public void Function_Local_And_Top_Level_Same_Name_Keep_Separate_Values()
    {
        var source =
            "identity n\n" +
            "    let h = n\n" +
            "    h\n" +
            "let h = 7\n" +
            "println identity 42\n" +
            "println h";

        var output = Run(source).Replace("\r\n", "\n").Trim();

        // 42 from the function's local h (= n), then 7 from the top-level h.
        output.Should().Be("42\n7");
    }
}
