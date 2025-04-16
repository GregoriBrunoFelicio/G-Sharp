using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Parsers;

namespace G.Sharp.Compiler.Tests.Parser;

public class ParseValidationsTests
{
    [Theory]
    [InlineData("x")]
    [InlineData("var123")]
    [InlineData("_var")]
    [InlineData("my_var2")]
    [InlineData("A")]
    [InlineData("CamelCase")]
    public void Should_Return_True_If_Variable_Name_Is_Valid(string name)
    {
        var result = Validations.IsValidVariableName(name);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("1var")]
    [InlineData("!var")]
    [InlineData(" ")]
    [InlineData("")]
    [InlineData("var-name")]
    [InlineData("x y")]
    public void Should_Return_False_If_Variable_Name_Is_Invalid(string name)
    {
        var result = Validations.IsValidVariableName(name);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("let")]
    [InlineData("if")]
    [InlineData("else")]
    [InlineData("while")]
    [InlineData("for")]
    [InlineData("return")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("null")]
    [InlineData("function")]
    [InlineData("print")]
    [InlineData("printf")]
    [InlineData("println")]
    [InlineData("string")]
    [InlineData("number")]
    [InlineData("boolean")]
    [InlineData("bool")]
    [InlineData("in")]
    [InlineData("int")]
    [InlineData("float")]
    [InlineData("char")]
    [InlineData("void")]
    public void Should_Return_True_If_Word_Is_Reserved(string word)
    {
        var result = Validations.IsReserved(word);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("myVar")]
    [InlineData("custom")]
    [InlineData("variable")]
    [InlineData("list")]
    [InlineData("Index")]
    [InlineData("Map")]
    public void Should_Return_False_If_Word_Is_Not_Reserved(string word)
    {
        var result = Validations.IsReserved(word);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(GPrimitiveType.Int)]
    [InlineData(GPrimitiveType.Float)]
    [InlineData(GPrimitiveType.Double)]
    [InlineData(GPrimitiveType.Decimal)]
    public void Should_Return_True_If_Number_Assigned_To_NumberSupertype(GPrimitiveType actualKind)
    {
        var expected = new GType(GPrimitiveType.Number);
        var actual = new GType(actualKind);

        var result = Validations.IsTypeCompatible(expected, actual);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(GPrimitiveType.String)]
    [InlineData(GPrimitiveType.Boolean)]
    [InlineData(GPrimitiveType.Number)]
    public void Should_Return_True_If_Kinds_Are_Exactly_The_Same(GPrimitiveType kind)
    {
        var expected = new GType(kind);
        var actual = new GType(kind);

        var result = Validations.IsTypeCompatible(expected, actual);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(GPrimitiveType.Int)]
    [InlineData(GPrimitiveType.Float)]
    public void Should_Return_False_If_Kinds_Different_And_Not_Supertype(GPrimitiveType actualKind)
    {
        var expected = new GType(GPrimitiveType.String);
        var actual = new GType(actualKind);

        var result = Validations.IsTypeCompatible(expected, actual);

        result.Should().BeFalse();
    }

    [Fact]
    public void Should_Return_False_If_Array_Types_Are_Different()
    {
        var expected = new GType(GPrimitiveType.Int, isArray: true);
        var actual = new GType(GPrimitiveType.Int, isArray: false);

        var result = Validations.IsTypeCompatible(expected, actual);

        result.Should().BeFalse();
    }
}