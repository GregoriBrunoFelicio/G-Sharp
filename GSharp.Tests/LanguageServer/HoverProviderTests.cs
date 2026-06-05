using FluentAssertions;
using GSharp.LanguageServer;

namespace G.Sharp.Compiler.Tests.LanguageServer;

public class HoverProviderTests
{
    // Coordinates are 1-based (matching the lexer/AST), the same convention HoverProvider uses.
    private static HoverResult? HoverAt(string source, int line, int character)
    {
        var analysis = DocumentAnalyzer.AnalyzeDocument(source);
        return HoverProvider.Find(analysis, line, character);
    }

    [Fact]
    public void Hovering_A_Function_Name_Shows_Its_Signature()
    {
        var source = "add a b => a + b\nlet r = add 3 5";

        // "add" declaration starts at column 1 on line 1.
        var hover = HoverAt(source, 1, 1);

        hover.Should().NotBeNull();
        hover!.Markdown.Should().Contain("add : int → int → int");
    }

    [Fact]
    public void Hovering_A_Call_Callee_Shows_The_Function_Signature()
    {
        var source = "add a b => a + b\nlet r = add 3 5";

        // The call "add" starts at column 9 on line 2 ("let r = add 3 5").
        var hover = HoverAt(source, 2, 9);

        hover.Should().NotBeNull();
        hover!.Markdown.Should().Contain("add : int → int → int");
    }

    [Fact]
    public void Hovering_A_Let_Binding_Shows_Its_Type()
    {
        var source = "let x = 42";

        // "x" is at column 5.
        var hover = HoverAt(source, 1, 5);

        hover.Should().NotBeNull();
        hover!.Markdown.Should().Contain("x : int");
    }

    [Fact]
    public void Hovering_A_Parameter_Use_Shows_Its_Inferred_Type()
    {
        var source = "add a b => a + b\nlet r = add 3 5";

        // The body's first "a" (the binding use) is at column 12 on line 1.
        var hover = HoverAt(source, 1, 12);

        hover.Should().NotBeNull();
        hover!.Markdown.Should().Contain("a : int");
    }

    [Fact]
    public void Hover_Range_Covers_The_Hovered_Identifier()
    {
        var source = "let x = 42";

        var hover = HoverAt(source, 1, 5);

        hover.Should().NotBeNull();
        hover!.Line.Should().Be(1);
        hover.StartColumn.Should().Be(5);
        hover.EndColumn.Should().Be(6); // single-character name "x"
    }

    [Fact]
    public void Hovering_Empty_Space_Returns_Null()
    {
        var source = "let x = 42";

        // Column 4 is the space before "x" — nothing typed lives there.
        HoverAt(source, 1, 4).Should().BeNull();
    }
}
