using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Parser;

public class PrintStatementTests
{
    [Fact]
    public void Should_Parse_Println_Statement()
    {
        var tokens = new List<Token>
        {
            new(TokenType.Println, "println"),
            new(TokenType.Identifier, "name"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.EndOfFile, "")
        };

        var parser = new Parsers.Parser(tokens);
        var result = parser.Parse();

        var let = result.Should()
            .ContainSingle()
            .Which
            .Should()
            .BeOfType<PrintStatement>()
            .Subject
            .As<PrintStatement>();

        let.VariableName.Should().Be("name");
    }
}