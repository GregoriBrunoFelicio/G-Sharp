using FluentAssertions;
using GSharp.AST;
using GSharp.Lexer;
using GSharp.Parser;


namespace G.Sharp.Compiler.Tests.Parser;

public class ExpressionParserTests
{
    [Fact]
    public void ExpressionParser_Should_Parse_Int_Literal_Expression()
    {
        var tokens = new List<Token>
        {
            new(TokenType.NumberLiteral, "123"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<LiteralExpression>();
        var literal = (LiteralExpression)result;
        literal.Value.Should().BeOfType<IntValue>();
        literal.Value.As<IntValue>().Value.Should().Be(123);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Float_Literal_Expression()
    {
        var tokens = new List<Token>
        {
            new(TokenType.NumberLiteral, "3.14f"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<LiteralExpression>();
        var literal = (LiteralExpression)result;
        literal.Value.Should().BeOfType<FloatValue>();
        literal.Value.As<FloatValue>().Value.Should().Be(3.14f);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_String_Literal_Expression()
    {
        var tokens = new List<Token>
        {
            new(TokenType.StringLiteral, "greg"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<LiteralExpression>();
        var literal = (LiteralExpression)result;
        literal.Value.Should().BeOfType<StringValue>();
        literal.Value.As<StringValue>().Value.Should().Be("greg");
    }

    [Theory]
    [InlineData(TokenType.BooleanTrueLiteral, true)]
    [InlineData(TokenType.BooleanFalseLiteral, false)]
    public void ExpressionParser_Should_Parse_Boolean_Literal_Expression(TokenType tokenType, bool expected)
    {
        var tokens = new List<Token>
        {
            new(tokenType, expected.ToString().ToLower()),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<LiteralExpression>();
        var literal = (LiteralExpression)result;
        literal.Value.Should().BeOfType<BooleanValue>();
        literal.Value.As<BooleanValue>().Value.Should().Be(expected);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Array_Of_Ints_Without_Commas()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "["),
            new(TokenType.NumberLiteral, "1"),
            new(TokenType.NumberLiteral, "2"),
            new(TokenType.NumberLiteral, "3"),
            new(TokenType.RightBracket, "]"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<LiteralExpression>();
        var array = result.As<LiteralExpression>().Value.As<ArrayValue>();

        array.Elements.Should().HaveCount(3);
        array.Elements[0].Should().BeOfType<IntValue>().Which.Value.Should().Be(1);
        array.Elements[1].Should().BeOfType<IntValue>().Which.Value.Should().Be(2);
        array.Elements[2].Should().BeOfType<IntValue>().Which.Value.Should().Be(3);
    }

    [Fact]
    public void ExpressionParser_Should_Throw_When_Array_Is_Empty()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "["),
            new(TokenType.RightBracket, "]"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var act = () => new ExpressionParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("Empty arrays are not supported.");
    }

    [Fact]
    public void ExpressionParser_Should_Throw_When_Array_Type_Cannot_Be_Inferred()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "["),
            new(TokenType.Identifier, "foo"),
            new(TokenType.RightBracket, "]"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var act = () => new ExpressionParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("Unable to infer array element type.");
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Variable_Expression()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Identifier, "myVar"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<VariableExpression>();
        result.As<VariableExpression>().Name.Should().Be("myVar");
    }

    [Fact]
    public void ExpressionParser_Should_Throw_When_Token_Is_Invalid()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Equals, "="),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var act = () => new ExpressionParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("*Unexpected token in expression*");
    }

    [Fact]
    public void Should_Parse_If_With_Variable_Declaration_Inside_Then()
    {
        var tokens = new List<Token>
        {
            new(TokenType.If, "if"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "42"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new IfParser(parser).Parse();

        result.ThenBody.Should().ContainSingle();
        result.ThenBody[0].Should().BeOfType<LetStatement>();
    }

    [Fact]
    public void Should_Parse_Let_Then_If_With_Assignment()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "10"),
            new(TokenType.Semicolon, ";"),

            new(TokenType.If, "if"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "99"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.RightBrace, "}"),

            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = parser.Parse();

        result.Should().HaveCount(2);
        result[0].Should().BeOfType<LetStatement>();
        result[1].Should().BeOfType<IfStatement>();

        var ifStmt = (IfStatement)result[1];
        ifStmt.ThenBody.Should().ContainSingle().Which.Should().BeOfType<AssignmentStatement>();
    }

    [Fact]
    public void Should_Parse_If_With_Comparison_Condition()
    {
        var tokens = new List<Token>
        {
            new(TokenType.If, "if"),
            new(TokenType.Identifier, "a"),
            new(TokenType.GreaterThan, ">"),
            new(TokenType.NumberLiteral, "5"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Println, "println"),
            new(TokenType.StringLiteral, "ok"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new IfParser(parser).Parse();

        result.Condition.Should().BeOfType<BinaryExpression>();
        result.ThenBody.Should().ContainSingle().Which.Should().BeOfType<PrintStatement>();
    }

    [Fact]
    public void Should_Parse_If_With_Logical_And_Condition()
    {
        var tokens = new List<Token>
        {
            new(TokenType.If, "if"),
            new(TokenType.Identifier, "x"),
            new(TokenType.And, "and"),
            new(TokenType.Identifier, "y"),
            new(TokenType.LeftBrace, "{"),
            new(TokenType.Println, "println"),
            new(TokenType.StringLiteral, "ok"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.RightBrace, "}"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new IfParser(parser).Parse();

        result.Condition.Should().BeOfType<BinaryExpression>();
        result.ThenBody.Should().ContainSingle().Which.Should().BeOfType<PrintStatement>();
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Chained_Binary_Expression()
    {
        var tokens = new List<Token>
        {
            new(TokenType.NumberLiteral, "1"),
            new(TokenType.GreaterThan, ">"),
            new(TokenType.NumberLiteral, "2"),
            new(TokenType.LessThan, "<"),
            new(TokenType.NumberLiteral, "3"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<BinaryExpression>();

        var outer = (BinaryExpression)result;
        outer.Operator.Should().Be(TokenType.LessThan);
        outer.Right.Should().BeOfType<LiteralExpression>();

        var inner = outer.Left.Should().BeOfType<BinaryExpression>().Subject;
        inner.Operator.Should().Be(TokenType.GreaterThan);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Boolean_And_Operator()
    {
        var tokens = new List<Token>
        {
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.And, "and"),
            new(TokenType.BooleanFalseLiteral, "false"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<BinaryExpression>();
        var expr = result.As<BinaryExpression>();
        expr.Operator.Should().Be(TokenType.And);
        expr.Left.Should().BeOfType<LiteralExpression>();
        expr.Right.Should().BeOfType<LiteralExpression>();
    }

    [Fact]
    public void ExpressionParser_Should_Throw_When_Expression_Ends_With_Operator()
    {
        var tokens = new List<Token>
        {
            new(TokenType.NumberLiteral, "10"),
            new(TokenType.EqualEqual, "=="),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var act = () => new ExpressionParser(parser).Parse();

        act.Should().Throw<Exception>().WithMessage("*Unexpected token in expression*");
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Array_Of_Booleans()
    {
        var tokens = new List<Token>
        {
            new(TokenType.LeftBracket, "["),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.BooleanFalseLiteral, "false"),
            new(TokenType.BooleanTrueLiteral, "true"),
            new(TokenType.RightBracket, "]"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<LiteralExpression>();
        var array = result.As<LiteralExpression>().Value.As<ArrayValue>();
        array.Elements.Should().HaveCount(3);
        array.Elements[0].As<BooleanValue>().Value.Should().BeTrue();
        array.Elements[1].As<BooleanValue>().Value.Should().BeFalse();
        array.Elements[2].As<BooleanValue>().Value.Should().BeTrue();
    }

    [Theory]
    [InlineData(TokenType.GreaterThan, ">", typeof(IntValue), typeof(IntValue))]
    [InlineData(TokenType.LessThan, "<", typeof(IntValue), typeof(IntValue))]
    [InlineData(TokenType.GreaterThanOrEqual, ">=", typeof(IntValue), typeof(IntValue))]
    [InlineData(TokenType.LessThanOrEqual, "<=", typeof(IntValue), typeof(IntValue))]
    [InlineData(TokenType.EqualEqual, "==", typeof(IntValue), typeof(IntValue))]
    [InlineData(TokenType.NotEqual, "!=", typeof(IntValue), typeof(IntValue))]
    [InlineData(TokenType.And, "and", typeof(BooleanValue), typeof(BooleanValue))]
    [InlineData(TokenType.Or, "or", typeof(BooleanValue), typeof(BooleanValue))]
    public void ExpressionParser_Should_Parse_BinaryExpression_With_Operator(
        TokenType opType, string opValue, Type leftType, Type rightType)
    {
        var leftToken = leftType == typeof(BooleanValue)
            ? new Token(TokenType.BooleanTrueLiteral, "true")
            : new Token(TokenType.NumberLiteral, "10");

        var rightToken = rightType == typeof(BooleanValue)
            ? new Token(TokenType.BooleanFalseLiteral, "false")
            : new Token(TokenType.NumberLiteral, "5");

        var tokens = new List<Token>
        {
            leftToken,
            new(opType, opValue),
            rightToken,
            new(TokenType.EndOfFile, "")
        };

        var parser = new GSharp.Parser.Parser(tokens);
        var result = new ExpressionParser(parser).Parse();

        result.Should().BeOfType<BinaryExpression>();
        var expr = (BinaryExpression)result;

        expr.Operator.Should().Be(opType);
        expr.Left.Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().BeOfType(leftType);
        expr.Right.Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().BeOfType(rightType);
    }
}