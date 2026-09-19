using FluentAssertions;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class MemberCallExecutionTests
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
    public void Member_On_A_Module_Call_Result()
    {
        Run("println time.now.year").Should().Be(DateTime.Now.Year.ToString());
    }

    [Fact]
    public void Member_On_A_Variable()
    {
        Run("d -> time.make 2026 9 19\nprintln d.month").Should().Be("9");
    }

    [Fact]
    public void Member_With_Arguments_On_A_Variable()
    {
        Run("d -> time.make 2026 9 19\nprintln (d.addDays 30).month").Should().Be("10");
    }

    [Fact]
    public void Member_On_A_Parenthesized_Expression_With_Arguments()
    {
        Run("println ((time.make 2026 9 19).addDays 30).day").Should().Be("19");
    }

    [Fact]
    public void Members_Chain()
    {
        Run("d -> time.make 2026 9 19\nprintln ((d.addDays 1).addDays 1).day").Should().Be("21");
    }

    [Fact]
    public void Array_And_String_Members()
    {
        Run("nums -> [1 2 3]\nprintln nums.len").Should().Be("3");
        Run("text -> \"abc\"\nprintln text.upper").Should().Be("ABC");
    }

    [Fact]
    public void Member_Result_Works_In_Arithmetic_With_Native_Fast_Path()
    {
        Run("d -> time.make 2026 9 19\nprintln d.year + 1").Should().Be("2027");
    }

    [Fact]
    public void Member_On_A_Function_Parameter()
    {
        Run("getYear d => d.year\nprintln (getYear (time.make 2026 9 19))").Should().Be("2026");
    }

    [Fact]
    public void Member_On_A_Lambda_Parameter()
    {
        Run("years -> array.map [1 2] (n => n.abs)\nprintln years").Should().Be("[1, 2]");
    }
}
