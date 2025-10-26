using FluentAssertions;
using GSharp.AST;
using GSharp.Parser;

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
}