using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;

namespace G.Sharp.Compiler.Tests.Parser;

public class PrintParserTests
{
    [Fact]
    public void Should_Return_PrintStatement_With_Expression()
    {
        var tokens = new List<Token>
        {
            new(TokenType.NumberLiteral, "42"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var printParser = new PrintParser(parser);

        var result = printParser.Parse();

        result.Should().BeOfType<PrintStatement>();

        var value = result.Expression.GetLiteralValue();
        value.Should().BeOfType<IntValue>();
        value.As<IntValue>().Value.Should().Be(42);
    }
}
