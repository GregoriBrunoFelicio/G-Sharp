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
    [InlineData(GPrimitiveType.Number, "string")]
    [InlineData(GPrimitiveType.Number, "boolean")]
    [InlineData(GPrimitiveType.String, "int")]
    [InlineData(GPrimitiveType.String, "boolean")]
    [InlineData(GPrimitiveType.Boolean, "int")]
    [InlineData(GPrimitiveType.Boolean, "string")]
    public void Should_Return_False_When_Primitive_Types_Do_Not_Match(GPrimitiveType expectedKind, string valueKind)
    {
        var expectedType = new GType(expectedKind);

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
    [InlineData(GPrimitiveType.Number, "int")]
    [InlineData(GPrimitiveType.Number, "float")]
    [InlineData(GPrimitiveType.Number, "double")]
    [InlineData(GPrimitiveType.Number, "decimal")]
    [InlineData(GPrimitiveType.String, "string")]
    [InlineData(GPrimitiveType.Boolean, "boolean")]
    public void Should_Return_True_When_Primitive_Types_Match(GPrimitiveType expectedKind, string valueKind)
    {
        var expectedType = new GType(expectedKind);

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
    
    [Fact]
    public void IsPrimitiveTypeCompatible_Should_Return_False_When_Value_Is_Array()
    {
        var expectedType = new GType(GPrimitiveType.String); 
        var arrayValue = new ArrayValue(
            Elements: new List<VariableValue> { new StringValue("greg"), new StringValue("ana") },
            ElementType: new GType(GPrimitiveType.String)
        );

        var result = Validations.IsTypeCompatible(expectedType, arrayValue);

        result.Should().BeFalse();
    }
}