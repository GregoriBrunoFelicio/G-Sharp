using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Parsers;

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
    public void IsReserved_Should_Return_True_For_Reserved_Keywords(string keyword)
    {
        var result = Validations.IsReserved(keyword);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("variable")]
    [InlineData("customName")]
    [InlineData("main")]
    [InlineData("calculateSum")]
    [InlineData("my_var_123")]
    public void IsReserved_Should_Return_False_For_Non_Reserved_Keywords(string keyword)
    {
        var result = Validations.IsReserved(keyword);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("LET")]
    [InlineData("If")]
    [InlineData("While")]
    [InlineData("Return")]
    [InlineData("False")]
    public void IsReserved_Should_Be_Case_Sensitive(string keyword)
    {
        var result = Validations.IsReserved(keyword);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void IsReserved_Should_Return_False_For_Empty_Or_Null(string keyword)
    {
        var result = Validations.IsReserved(keyword);
        result.Should().BeFalse();
    }
    
    [Theory]
    [InlineData(GType.Number, "string")]
    [InlineData(GType.Number, "boolean")]
    [InlineData(GType.String, "int")]
    [InlineData(GType.String, "boolean")]
    [InlineData(GType.Boolean, "int")]
    [InlineData(GType.Boolean, "string")]
    public void IsTypeCompatible_Should_Return_False_When_Types_Do_Not_Match(GType expectedType, string valueKind)
    {
        VariableValue value = valueKind switch
        {
            "int" => new IntValue(1),
            "float" => new FloatValue(1.0f),
            "double" => new DoubleValue(1.0),
            "decimal" => new DecimalValue(1.0m),
            "string" => new StringValue("mismatch"),
            "boolean" => new BooleanValue(false),
            _ => throw new InvalidOperationException("Unknown value kind")
        };

        var result = Validations.IsTypeCompatible(expectedType, value);
        result.Should().BeFalse();
    }
    
    [Theory]
    [InlineData(GType.Number, "int")]
    [InlineData(GType.Number, "float")]
    [InlineData(GType.Number, "double")]
    [InlineData(GType.Number, "decimal")]
    [InlineData(GType.String, "string")]
    [InlineData(GType.Boolean, "boolean")]
    public void IsTypeCompatible_Should_Return_True_When_Types_Match(GType expectedType, string valueKind)
    {
        VariableValue value = valueKind switch
        {
            "int" => new IntValue(1),
            "float" => new FloatValue(1.0f),
            "double" => new DoubleValue(1.0),
            "decimal" => new DecimalValue(1.0m),
            "string" => new StringValue("ok"),
            "boolean" => new BooleanValue(true),
            _ => throw new ArgumentOutOfRangeException(nameof(valueKind), valueKind, null)
        };

        var result = Validations.IsTypeCompatible(expectedType, value);
        result.Should().BeTrue();
    }
}