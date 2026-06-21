using FluentAssertions;
using GSharp.TypeChecker;

namespace G.Sharp.Compiler.Tests.TypeChecker;

public class TypeInferrerTests
{
    private static Dictionary<GSharp.AST.Expression, GsType> Infer(string source)
    {
        var tokens = new GSharp.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Parser.Parser(tokens).Parse();
        return new TypeInferrer().Infer(expressions);
    }

    private static void ShouldThrowTypeMismatch(string source, string expectedFragment)
    {
        var act = () => Infer(source);
        act.Should().Throw<Exception>().WithMessage($"*{expectedFragment}*");
    }

    // -------------------------------------------------------------------------
    // Literal inference
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Infer_Int_Literal()
    {
        var types = Infer("x -> 42");

        types.Values.Should().ContainItemsAssignableTo<IntType>();
    }

    [Fact]
    public void Should_Infer_Double_Literal()
    {
        var types = Infer("x -> 3.14d");

        types.Values.Should().ContainItemsAssignableTo<DoubleType>();
    }

    [Fact]
    public void Should_Infer_String_Literal()
    {
        var types = Infer("x -> \"hello\"");

        types.Values.Should().ContainItemsAssignableTo<StringType>();
    }

    [Fact]
    public void Should_Infer_Bool_Literal()
    {
        var types = Infer("x -> true");

        types.Values.Should().ContainItemsAssignableTo<BoolType>();
    }

    // -------------------------------------------------------------------------
    // Arithmetic inference
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Infer_Int_Addition()
    {
        var types = Infer("x -> 10 + 20");

        types.Values.Should().ContainItemsAssignableTo<IntType>();
    }

    [Fact]
    public void Should_Infer_Double_Addition()
    {
        var types = Infer("x -> 1.0d + 2.0d");

        types.Values.Should().ContainItemsAssignableTo<DoubleType>();
    }

    [Fact]
    public void Should_Infer_Chained_Arithmetic()
    {
        // x + y + z where all are int — should resolve to int without error
        var act = () => Infer("x -> 1\ny -> 2\nz -> x + y + 3");
        act.Should().NotThrow();
    }

    // -------------------------------------------------------------------------
    // Function inference
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Infer_Function_Return_Type_From_Body()
    {
        var act = () => Infer("double x => x * 2\nresult -> double 5");
        act.Should().NotThrow();
    }

    [Fact]
    public void Should_Infer_Recursive_Function()
    {
        var source = """
            factorial n
                if n <= 1 then 1 else n * factorial(n - 1)
            result -> factorial 5
            """;

        var act = () => Infer(source);
        act.Should().NotThrow();
    }

    // -------------------------------------------------------------------------
    // println returns unit
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Record_Println_As_Unit_In_Type_Map()
    {
        var types = Infer("println \"hello\"");

        types.Values.Should().Contain(t => t is UnitType);
    }

    // -------------------------------------------------------------------------
    // Type errors
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Throw_On_Int_Plus_String()
    {
        ShouldThrowTypeMismatch("x -> 10 + \"hello\"", "type mismatch");
    }

    [Fact]
    public void Should_Throw_On_Mismatched_If_Branches()
    {
        ShouldThrowTypeMismatch(
            "flag -> true\nx -> if flag then 1 else \"text\"",
            "type mismatch");
    }

    [Fact]
    public void Should_Throw_On_Int_Plus_Double()
    {
        ShouldThrowTypeMismatch("x -> 10 + 3.14d", "type mismatch");
    }

    // -------------------------------------------------------------------------
    // Type ToString display
    // -------------------------------------------------------------------------

    [Fact]
    public void GsType_ToString_Should_Be_Human_Readable()
    {
        new IntType().ToString().Should().Be("int");
        new DoubleType().ToString().Should().Be("double");
        new FloatType().ToString().Should().Be("float");
        new StringType().ToString().Should().Be("string");
        new BoolType().ToString().Should().Be("bool");
        new UnitType().ToString().Should().Be("unit");
        new ArrayType(new IntType()).ToString().Should().Be("[int]");
        new FunctionType(new IntType(), new BoolType()).ToString().Should().Be("(int → bool)");
        new TypeVar("a").ToString().Should().Be("'a");
    }
}
