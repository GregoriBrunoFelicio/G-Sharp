using FluentAssertions;
using GSharp.Compiler.AST;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.TypeChecker;

public class MemberCallTypeInferenceTests
{
    private static GsType InferLastBindingType(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        var types = new TypeInferrer().Infer(expressions);
        return types[((BindingExpression)expressions[^1]).Value];
    }

    private static void ShouldThrow(string source, string expectedFragment)
    {
        var act = () => InferLastBindingType(source);
        act.Should().Throw<Exception>().WithMessage($"*{expectedFragment}*");
    }

    [Fact]
    public void Date_Member_Resolves_To_The_Time_Module()
    {
        InferLastBindingType("y -> time.now.year").Should().BeOfType<IntType>();
        InferLastBindingType("d -> time.now\nnext -> d.addDays 1").Should().BeOfType<DateType>();
    }

    [Fact]
    public void Array_And_String_Receivers_Resolve_By_Type()
    {
        InferLastBindingType("nums -> [1 2 3]\nn -> nums.len").Should().BeOfType<IntType>();
        InferLastBindingType("text -> \"abc\"\nn -> text.len").Should().BeOfType<IntType>();
        InferLastBindingType("text -> \"abc\"\nup -> text.upper").Should().BeOfType<StringType>();
    }

    [Fact]
    public void Member_Missing_On_The_Receiver_Type_Is_An_Error()
    {
        ShouldThrow("d -> time.now\nx -> d.upper", "has no member 'upper'");
    }

    [Fact]
    public void Wrong_Argument_Type_Is_Still_Checked()
    {
        ShouldThrow("d -> time.now\nx -> d.addDays \"a\"", "type mismatch");
    }

    [Fact]
    public void Unknown_Receiver_Type_Resolves_A_Member_Unique_To_One_Module()
    {
        InferLastBindingType("getYear d => d.year\ny -> getYear (time.now)").Should().BeOfType<IntType>();
    }

    [Fact]
    public void Unknown_Receiver_Type_With_An_Ambiguous_Member_Is_An_Error()
    {
        ShouldThrow("size x => x.len\ny -> size \"a\"", "cannot resolve '.len'");
    }

    [Fact]
    public void A_Module_Wins_Over_A_Variable_Of_The_Same_Name()
    {
        InferLastBindingType("time -> 5\nn -> time.make 2026 1 1").Should().BeOfType<DateType>();
    }
}
