using FluentAssertions;
using GSharp.TypeChecker;

namespace G.Sharp.Compiler.Tests.Interop;

// End-to-end: `import system.math` lets G# call static .NET methods, with overloads resolved
// from the inferred argument types.
public class DotnetInteropExecutionTests
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

        return captured.ToString().Replace("\r\n", "\n").Trim();
    }

    [Fact]
    public void Calls_A_Static_Method_With_A_Double_Argument()
    {
        var output = Run("import system.math\nprintln math.sqrt 16.0d");

        output.Should().Be("4");
    }

    [Fact]
    public void Resolves_An_Int_Overload_From_Argument_Types()
    {
        // Math.Max has many overloads; the int,int one is picked from the inferred arg types.
        var output = Run("import system.math\nprintln math.max 3 5");

        output.Should().Be("5");
    }

    [Fact]
    public void Calls_A_Method_With_Two_Double_Arguments()
    {
        var output = Run("import system.math\nprintln math.pow 2.0d 10.0d");

        output.Should().Be("1024");
    }

    [Fact]
    public void Alias_Lets_A_Reserved_Type_Name_Be_Called()
    {
        // `string` is reserved, so it can't be used at a call site — the alias `str` sidesteps it.
        var output = Run("import system.string as str\nprintln str.concat \"Hello, \" \"World\"");

        output.Should().Be("Hello, World");
    }
}
