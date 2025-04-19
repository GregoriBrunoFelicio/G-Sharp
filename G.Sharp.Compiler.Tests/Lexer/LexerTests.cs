using FluentAssertions;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Tests.Lexer;

public class LexerTests
{
    [Theory, MemberData(nameof(GetTokenizationSamples))]
    public void Should_Tokenize_Code_Correctly(string code)
    {
        var lexer = new Compiler.Lexer.Lexer(code);
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
        var act = () => new Compiler.Lexer.Lexer(code);

        act.Should().Throw<NullReferenceException>()
            .WithMessage("Code cannot be null or empty.");
    }

    [Fact]
    public void Should_Advance_Position()
    {
        var lexer = new Compiler.Lexer.Lexer("a");
        lexer.Position.Should().Be(0);

        lexer.Advance();
        lexer.Position.Should().Be(1);
    }

    [Fact]
    public void Should_Return_True_When_End_Is_Reached()
    {
        var lexer = new Compiler.Lexer.Lexer("a");
        lexer.Advance();
        lexer.IsAtEnd().Should().BeTrue();
    }

    [Fact]
    public void Should_Peek_Next_Character_Correctly()
    {
        var lexer = new Compiler.Lexer.Lexer("abc");
        lexer.Next().Should().Be('b');
    }

    [Fact]
    public void Should_Advance_While_Condition_Is_True()
    {
        var lexer = new Compiler.Lexer.Lexer("    xyz");
        lexer.AdvanceWhile(char.IsWhiteSpace);
        lexer.Current.Should().Be('x');
    }

    public static IEnumerable<object[]> GetTokenizationSamples()
    {
        yield return ["""let name: string = "greg";"""];
        yield return ["let age: number = 33;"];
        yield return ["let isTrue: bool = true;"];
        yield return ["println name;"];
        yield return ["let variable:number = 10; variable = 20; println variable;"];
        yield return ["let d: number = 10.13d;"];
        yield return ["let m: number = 10.24m;"];
        yield return ["let f: number = 10.87f;"];
        yield return ["let nums: number[] = [1 2 3];"];
        yield return ["""let names: string[] = ["Greg" "Felicio"];"""];
        yield return ["let flags: bool[] = [true false true];"];
        yield return ["for item in nums { println item; }"];
        yield return ["""if 1 == 1 { println "ok"; }"""];
        yield return ["""if n != 10 { println "x"; } else { println "y"; }"""];
        yield return ["""if a >= 5 { println "a >= 5"; }"""];
        yield return ["""if b <= a { println "b <= a"; }"""];
        yield return ["""if a == 10 { println "ok"; }"""];
        yield return ["""if value < 5 { println "less"; }"""];
        yield return ["""if value > 1 { println "greater"; }"""];
        yield return ["""let isFalse: bool = false; if isFalse { println "no"; } else { println "yes"; }"""];
        yield return ["""let isTrue: bool = true; if isTrue { println "yes"; } else { println "no"; }"""];
        yield return ["let empty: number[] = [];"];
        yield return ["""if isTrue and 1 == 1 { println "compound"; }"""];
        yield return ["""if not isFalse { println "negated"; }"""];
        yield return ["""if isTrue or isFalse { println "logic"; }"""];
    }
}