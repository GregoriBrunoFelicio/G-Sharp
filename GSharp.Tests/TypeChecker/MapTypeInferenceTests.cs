using FluentAssertions;
using GSharp.Compiler.AST;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.TypeChecker;

public class MapTypeInferenceTests
{
    private static GsType InferMapBindingType(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        var types = new TypeInferrer().Infer(expressions);
        var mapExpression = ((BindingExpression)expressions[0]).Value;
        return types[mapExpression];
    }

    private static void ShouldThrowTypeMismatch(string source, string expectedFragment)
    {
        var act = () =>
        {
            var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
            var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
            new TypeInferrer().Infer(expressions);
        };
        act.Should().Throw<Exception>().WithMessage($"*{expectedFragment}*");
    }

    [Fact]
    public void Map_Literal_Infers_Key_And_Value_Types()
    {
        var type = InferMapBindingType("m -> {\"a\": 1 \"b\": 2}");

        var mapType = type.Should().BeOfType<MapType>().Subject;
        mapType.KeyType.Should().BeOfType<StringType>();
        mapType.ValueType.Should().BeOfType<IntType>();
    }

    [Fact]
    public void Empty_Map_Literal_Infers_A_Map_Type()
    {
        var type = InferMapBindingType("m -> {}");

        type.Should().BeOfType<MapType>();
    }

    [Fact]
    public void Mismatched_Value_Types_Across_Pairs_Throw()
    {
        ShouldThrowTypeMismatch("m -> {\"a\": 1 \"b\": 2.0d}", "type mismatch");
    }

    [Fact]
    public void Mismatched_Key_Types_Across_Pairs_Throw()
    {
        ShouldThrowTypeMismatch("m -> {\"a\": 1 2: 2}", "type mismatch");
    }

    [Fact]
    public void Map_Get_Returns_The_Value_Type()
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer("m -> {\"a\": 1}\nn -> map.get m \"a\"").Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        var types = new TypeInferrer().Infer(expressions);
        var getCall = ((BindingExpression)expressions[1]).Value;

        types[getCall].Should().BeOfType<IntType>();
    }

    [Fact]
    public void Map_Set_Preserves_The_Map_Type()
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(
            "m -> {\"a\": 1}\nm2 -> map.set m \"b\" 2").Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        var types = new TypeInferrer().Infer(expressions);
        var setCall = ((BindingExpression)expressions[1]).Value;

        var mapType = types[setCall].Should().BeOfType<MapType>().Subject;
        mapType.KeyType.Should().BeOfType<StringType>();
        mapType.ValueType.Should().BeOfType<IntType>();
    }

    [Fact]
    public void Map_Set_With_Wrong_Value_Type_Throws()
    {
        ShouldThrowTypeMismatch("m -> {\"a\": 1}\nm2 -> map.set m \"b\" \"oops\"", "type mismatch");
    }

    [Fact]
    public void Map_Keys_Returns_An_Array_Of_The_Key_Type()
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer("m -> {\"a\": 1}\nk -> map.keys m").Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        var types = new TypeInferrer().Infer(expressions);
        var keysCall = ((BindingExpression)expressions[1]).Value;

        var arrayType = types[keysCall].Should().BeOfType<ArrayType>().Subject;
        arrayType.ElementType.Should().BeOfType<StringType>();
    }

    [Fact]
    public void GsType_ToString_Renders_Map_Type()
    {
        new MapType(new StringType(), new IntType()).ToString().Should().Be("{string: int}");
    }
}
