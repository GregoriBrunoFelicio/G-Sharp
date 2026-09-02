using FluentAssertions;
using GSharp.Compiler.AST;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.TypeChecker;

public class LambdaTypeInferenceTests
{
    private static (List<Expression> Expressions, Dictionary<Expression, GsType> Types) Infer(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        return (expressions, new TypeInferrer().Infer(expressions));
    }

    private static GsType BoundValueType(string source)
    {
        var (expressions, types) = Infer(source);
        return types[((BindingExpression)expressions[0]).Value];
    }

    [Fact]
    public void Map_Can_Change_The_Element_Type()
    {
        BoundValueType("result -> array.map [1 2 3] (n => n > 2)")
            .Should().Be(new ArrayType(new BoolType()));
    }

    [Fact]
    public void Filter_Preserves_The_Element_Type()
    {
        BoundValueType("result -> array.filter [1 2 3] (n => n > 2)")
            .Should().Be(new ArrayType(new IntType()));
    }

    [Fact]
    public void Fold_Returns_The_Accumulator_Type()
    {
        BoundValueType("result -> array.fold [1 2 3] 0 (acc elem => acc + elem)")
            .Should().Be(new IntType());
    }

    [Fact]
    public void Fold_With_Wrong_Arity_Throws()
    {
        var act = () => Infer("result -> array.fold [1 2 3] 0");

        act.Should().Throw<Exception>().WithMessage("*array.fold*3*");
    }

    [Fact]
    public void Lambda_Body_Referencing_An_Undefined_Name_Throws()
    {
        var act = () => Infer("f -> n => x");

        act.Should().Throw<Exception>().WithMessage("*'x' is not defined*");
    }
}
