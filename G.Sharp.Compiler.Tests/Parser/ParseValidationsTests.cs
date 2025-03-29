using FluentAssertions;
using G.Sharp.Compiler.Parser;

namespace G.Sharp.Compiler.Tests.Parser;

public class ParseValidationsTests
{
    [Theory]
    [InlineData("name")]
    [InlineData("my_variable")]
    [InlineData("variable1")]
    [InlineData("user_input_2")]
    [InlineData("_internal")]
    [InlineData("validName123")]
    public void Should_Accept_Valid_Names(string variableName)
    {
        var result = Validations.IsValidVariableName(variableName);
        result.Should().BeTrue();

    }

    [Theory]
    [InlineData("1name")]
    [InlineData("no$me")]
    [InlineData("var#123")]
    [InlineData("#hidden")]
    [InlineData("with space")]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("name!")]
    public void Should_Reject_Invalid_Names(string variableName)
    {
        var result = Validations.IsValidVariableName(variableName);
        result.Should().BeFalse();
    }
}