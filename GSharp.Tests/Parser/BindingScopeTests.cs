using FluentAssertions;

namespace G.Sharp.Compiler.Tests.Scoping;

// Bindings are scoped per function: a name declared inside a function never collides with the
// same name at the top level or in another function. Redeclaring a name within the same scope
// stays an error (let is immutable — no reassignment, no same-scope shadowing).
public class BindingScopeTests
{
    private static Action Parsing(string source) => () =>
    {
        var tokens = new GSharp.Lexer.Lexer(source).Tokenize();
        _ = new GSharp.Parser.Parser(tokens).Parse();
    };

    [Fact]
    public void Same_Name_Inside_Function_And_At_Top_Level_Is_Allowed()
    {
        var source =
            "identity x\n" +
            "    let h = x\n" +
            "    h\n" +
            "let h = 10";

        Parsing(source).Should().NotThrow();
    }

    [Fact]
    public void Same_Name_In_Two_Different_Functions_Is_Allowed()
    {
        var source =
            "first x\n" +
            "    let h = x\n" +
            "    h\n" +
            "second y\n" +
            "    let h = y\n" +
            "    h";

        Parsing(source).Should().NotThrow();
    }

    [Fact]
    public void Redeclaring_At_Top_Level_Throws()
    {
        var source = "let x = 1\nlet x = 2";

        Parsing(source).Should().Throw<Exception>().WithMessage("*already declared*");
    }

    [Fact]
    public void Redeclaring_In_The_Same_Function_Throws()
    {
        var source =
            "fn a\n" +
            "    let x = a\n" +
            "    let x = a";

        Parsing(source).Should().Throw<Exception>().WithMessage("*already declared*");
    }

    [Fact]
    public void Redeclaring_Across_Blocks_In_The_Same_Function_Throws()
    {
        // `if`/`for` don't open a new scope — both `let x` land in the function scope.
        var source =
            "fn a\n" +
            "    if a > 0 then\n" +
            "        let x = a\n" +
            "        x\n" +
            "    else\n" +
            "        a\n" +
            "    let x = a\n" +
            "    x";

        Parsing(source).Should().Throw<Exception>().WithMessage("*already declared*");
    }

    [Fact]
    public void A_Let_Shadowing_A_Parameter_Throws()
    {
        var source =
            "fn x\n" +
            "    let x = 1\n" +
            "    x";

        Parsing(source).Should().Throw<Exception>().WithMessage("*already declared*");
    }
}
