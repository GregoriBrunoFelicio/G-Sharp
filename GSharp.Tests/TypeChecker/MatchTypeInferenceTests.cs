using FluentAssertions;
using GSharp.Compiler.AST;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.TypeChecker;

public class MatchTypeInferenceTests
{
    private static Dictionary<Expression, GsType> Infer(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        return new TypeInferrer().Infer(expressions);
    }

    private static void ShouldThrowTypeMismatch(string source, string expectedFragment)
    {
        var act = () => Infer(source);
        act.Should().Throw<Exception>().WithMessage($"*{expectedFragment}*");
    }

    [Fact]
    public void Match_Infers_The_Common_Arm_Body_Type()
    {
        var types = Infer("x -> match 1\n    1 => \"one\"\n    n => \"other\"");

        types.Values.Should().ContainItemsAssignableTo<StringType>();
    }

    [Fact]
    public void Catch_All_Binds_The_Scrutinee_Type()
    {
        var types = Infer("x -> match 1\n    n => n + 1");

        types.Values.Should().ContainItemsAssignableTo<IntType>();
    }

    [Fact]
    public void Literal_Pattern_Type_Mismatch_With_Scrutinee_Throws()
    {
        ShouldThrowTypeMismatch(
            "x -> match 1\n    \"a\" => 1\n    n => 2",
            "type mismatch");
    }

    [Fact]
    public void Mismatched_Arm_Body_Types_Throw()
    {
        ShouldThrowTypeMismatch(
            "x -> match 1\n    1 => \"one\"\n    n => 2",
            "type mismatch");
    }
}
