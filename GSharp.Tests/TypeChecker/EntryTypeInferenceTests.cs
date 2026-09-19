using FluentAssertions;
using GSharp.Compiler.AST;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.TypeChecker;

public class EntryTypeInferenceTests
{
    private static (List<Expression> Expressions, Dictionary<Expression, GsType> Types) Infer(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        return (expressions, new TypeInferrer().Infer(expressions));
    }

    private static GsType LastBindingType(string source)
    {
        var (expressions, types) = Infer(source);
        return types[((BindingExpression)expressions[^1]).Value];
    }

    private static void ShouldThrow(string source, string expectedFragment)
    {
        var act = () => Infer(source);
        act.Should().Throw<Exception>().WithMessage($"*{expectedFragment}*");
    }

    [Fact]
    public void Entries_Of_A_Map_Are_An_Array_Of_Entry()
    {
        LastBindingType("m -> {\"a\": 1}\nes -> map.entries m")
            .Should().Be(new ArrayType(new EntryType(new StringType(), new IntType())));
    }

    [Fact]
    public void For_Over_A_Map_Binds_An_Entry()
    {
        var (expressions, types) = Infer("m -> {\"a\": 1}\nresult -> for e in m do\n    e.value\n");
        var forExpression = (ForExpression)((BindingExpression)expressions[^1]).Value;

        types[forExpression.Body[0]].Should().BeOfType<IntType>();
        types[forExpression].Should().Be(new ArrayType(new IntType()));
    }

    [Fact]
    public void Entry_Key_And_Value_Have_The_Map_Types()
    {
        LastBindingType("m -> {\"a\": 1.5d}\nes -> map.entries m\nk -> array.head es\nn -> k.key")
            .Should().BeOfType<StringType>();
        LastBindingType("m -> {\"a\": 1.5d}\nes -> map.entries m\nk -> array.head es\nn -> k.value")
            .Should().BeOfType<DoubleType>();
    }

    [Fact]
    public void GetOr_Returns_The_Value_Type()
    {
        LastBindingType("m -> {\"a\": 1}\nn -> map.getOr m \"b\" 0").Should().BeOfType<IntType>();
    }

    [Fact]
    public void GetOr_Fallback_Must_Match_The_Value_Type()
    {
        ShouldThrow("m -> {\"a\": 1}\nn -> map.getOr m \"b\" \"zero\"", "type mismatch");
    }

    [Fact]
    public void For_Over_An_Array_Is_Unchanged()
    {
        var (expressions, types) = Infer("nums -> [1 2]\nresult -> for n in nums do\n    n + 1\n");
        var forExpression = (ForExpression)((BindingExpression)expressions[^1]).Value;

        types[forExpression].Should().Be(new ArrayType(new IntType()));
    }

    [Fact]
    public void Entry_Member_Is_Unique_So_It_Resolves_On_A_Lambda_Parameter()
    {
        LastBindingType("m -> {\"a\": 1}\nvalues -> array.map (map.entries m) (e => e.value)")
            .Should().Be(new ArrayType(new IntType()));
    }
}
