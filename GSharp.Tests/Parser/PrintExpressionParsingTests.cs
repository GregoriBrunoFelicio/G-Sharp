using FluentAssertions;
using GSharp.Compiler.AST;
using GSharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Parser;

public class PrintExpressionParsingTests
{
    private static PrintExpression Parsed(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        return (PrintExpression)expressions[0];
    }

    [Fact]
    public void Println_Produces_A_PrintExpression_With_The_Println_Keyword()
    {
        Parsed("println \"hi\"").Keyword.Should().Be(TokenType.Println);
    }

    [Fact]
    public void Print_Produces_A_PrintExpression_With_The_Print_Keyword()
    {
        Parsed("print \"hi\"").Keyword.Should().Be(TokenType.Print);
    }

    [Fact]
    public void Print_Parses_Its_Operand_The_Same_Way_As_Println()
    {
        Parsed("print 1 + 1").Value.Should().BeOfType<BinaryExpression>();
    }
}
