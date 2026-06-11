using FluentAssertions;
using GSharp.Lexer;

namespace G.Sharp.Compiler.Tests.Lexer;

public class LexerTests
{
    [Theory, MemberData(nameof(GetTokenizationSamples))]
    public void Should_Tokenize_Code_Correctly(string code)
    {
        var lexer = new GSharp.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();

        tokens.Should().NotBeNull();
        tokens.Should().NotBeEmpty();
        tokens.Last().Type.Should().Be(TokenType.EndOfFile);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t\r")]
    public void Should_Throw_When_Code_Is_Null_Or_Whitespace(string code)
    {
        var act = () => new GSharp.Lexer.Lexer(code);

        act.Should().Throw<NullReferenceException>()
            .WithMessage("Code cannot be null or empty.");
    }

    [Fact]
    public void Should_Advance_Position()
    {
        var lexer = new GSharp.Lexer.Lexer("a");
        lexer.Position.Should().Be(0);

        lexer.Advance();
        lexer.Position.Should().Be(1);
    }

    [Fact]
    public void Should_Return_True_When_End_Is_Reached()
    {
        var lexer = new GSharp.Lexer.Lexer("a");
        lexer.Advance();
        lexer.IsAtEnd().Should().BeTrue();
    }

    [Fact]
    public void Should_Peek_Next_Character_Correctly()
    {
        var lexer = new GSharp.Lexer.Lexer("abc");
        lexer.Next().Should().Be('b');
    }

    [Fact]
    public void Should_Advance_While_Condition_Is_True()
    {
        var lexer = new GSharp.Lexer.Lexer("    xyz");
        lexer.AdvanceWhile(char.IsWhiteSpace);
        lexer.Current.Should().Be('x');
    }

    public static IEnumerable<object[]> GetTokenizationSamples()
    {
        yield return ["""let name = "greg" """];
        yield return ["let age = 33"];
        yield return ["let isTrue = true"];
        yield return ["println name"];
        yield return ["let d = 10.13d"];
        yield return ["let m = 10.24m"];
        yield return ["let f = 10.87f"];
        yield return ["let nums = [1 2 3]"];
        yield return ["""let names = ["Greg" "Felicio"]"""];
        yield return ["let flags = [true false true]"];
        yield return ["for item in nums do\n    println item"];
        yield return ["""if 1 == 1 then println "ok" """];
        yield return ["""if n != 10 then println "x" else println "y" """];
        yield return ["add a b => a + b"];
        yield return ["square x => x * x"];
        yield return ["import mymodule"];
        yield return ["let empty = []"];
        yield return ["""if isTrue and 1 == 1 then println "compound" """];
        yield return ["""if not isFalse then println "negated" """];
        yield return ["""if isTrue or isFalse then println "logic" """];
    }
}