using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Parser;

public class ParserTests
{
    [Fact]
    public void Parse_Should_Return_Three_Distinct_Statements_When_Input_Is_Valid()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "1"),
            new(TokenType.Semicolon, ";"),

            new(TokenType.Identifier, "x"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "2"),
            new(TokenType.Semicolon, ";"),

            new(TokenType.Println, "println"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Semicolon, ";"),

            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        result.Should().HaveCount(3);
        result[0].Should().BeOfType<LetStatement>();
        result[1].Should().BeOfType<AssignmentStatement>();
        result[2].Should().BeOfType<PrintStatement>();
    }

    [Fact]
    public void Parse_Should_Throw_When_Colon_Token_Is_Missing_In_Let_Declaration()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "10"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => parser.Parse();

        act.Should().Throw<Exception>().WithMessage("*Expected token Colon*");
    }

    [Fact]
    public void Parse_Should_Throw_When_Unexpected_Token_Is_Found_As_Statement_Start()
    {
        var tokens = new List<Token>
        {
            new(TokenType.NumberLiteral, "99"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => parser.Parse();

        act.Should().Throw<Exception>().WithMessage("*Invalid statement*");
    }

    [Fact]
    public void Parse_Should_Throw_When_Let_Statement_Is_Truncated_Before_Equals()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => parser.Parse();

        act.Should().Throw<Exception>().WithMessage("*Expected token Equals*");
    }

    [Fact]
    public void Parse_Should_Succeed_When_Multiple_EndOfFile_Tokens_Are_Present()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Colon, ":"),
            new(TokenType.Number, "number"),
            new(TokenType.Equals, "="),
            new(TokenType.NumberLiteral, "5"),
            new(TokenType.Semicolon, ";"),

            new(TokenType.EndOfFile, ""),
        };

        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        result.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_Should_Handle_Let_With_String_Type()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "name"),
            new(TokenType.Colon, ":"),
            new(TokenType.String, "string"),
            new(TokenType.Equals, "="),
            new(TokenType.StringLiteral, "greg"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        result.Should().HaveCount(1);
        result[0].Should().BeOfType<LetStatement>();
        var let = (LetStatement)result[0];
        let.VariableName.Should().Be("name");
        let.VariableValue.Should().BeOfType<StringValue>();
    }

    [Fact]
    public void Parse_Should_Handle_For_Loop()
    {
        var tokens = new Compiler.Lexer.Lexer("for item in items { println item; }").Tokenize();
        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        result.Should().HaveCount(1);
        result[0].Should().BeOfType<ForStatement>();
        var forStmt = (ForStatement)result[0];
        forStmt.Variable.Should().Be("item");
        forStmt.Iterable.Should().BeOfType<VariableExpression>();
    }

    [Fact]
    public void Parse_Should_Handle_Let_Then_Println_Using_Variable()
    {
        var code = """
                   let x: number = 10;
                   println x;
                   """;
        var tokens = new Compiler.Lexer.Lexer(code).Tokenize();
        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        result.Should().HaveCount(2);
        result[0].Should().BeOfType<LetStatement>();
        result[1].Should().BeOfType<PrintStatement>();
    }

    [Fact]
    public void Parse_Should_Throw_When_Unexpected_Keyword_Appears()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Println, "println"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var act = () => parser.Parse();

        act.Should().Throw<Exception>()
            .WithMessage("*Expected token Identifier*");
    }
}