using FluentAssertions;
using GSharp.LanguageServer;

namespace G.Sharp.Compiler.Tests.LanguageServer;

public class DocumentAnalyzerTests
{
    [Fact]
    public void Valid_Source_Produces_No_Diagnostics()
    {
        var diagnostics = DocumentAnalyzer.Analyze("x -> 42");

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Empty_Source_Produces_No_Diagnostics()
    {
        DocumentAnalyzer.Analyze("").Should().BeEmpty();
    }

    [Fact]
    public void Whitespace_Only_Source_Produces_No_Diagnostics()
    {
        DocumentAnalyzer.Analyze("   \n\n   ").Should().BeEmpty();
    }

    [Fact]
    public void Syntax_Error_Maps_To_Its_Line()
    {
        // '@' is not a valid token; the lexer reports it on line 2 (1-based).
        var source = "x -> 42\n@";

        var diagnostics = DocumentAnalyzer.Analyze(source);

        diagnostics.Should().ContainSingle();
        var diagnostic = diagnostics[0];
        diagnostic.Line.Should().Be(1); // 0-based -> second line
        diagnostic.Severity.Should().Be(AnalyzerSeverity.Error);
        // The "2:" position prefix is stripped from the shown message.
        diagnostic.Message.Should().NotStartWith("2:");
        diagnostic.Message.Should().Contain("unexpected");
    }

    [Fact]
    public void Indentation_Error_Maps_To_Its_Block_Line()
    {
        // The body dedents to an unexpected level, so the offending token is a block
        // marker. Block tokens carry their source line, so the diagnostic lands on the
        // bad line (line 4, 1-based) instead of falling back to the first line.
        var source =
            "f x\n" +
            "    a -> 1\n" +
            "  b -> 2\n" +
            "        c -> 3\n";

        var diagnostics = DocumentAnalyzer.Analyze(source);

        diagnostics.Should().ContainSingle();
        diagnostics[0].Line.Should().BeGreaterThan(0); // not the line-1 fallback
    }

    [Fact]
    public void Undefined_Variable_Maps_To_Its_Line()
    {
        // 'y' is never bound; the type inferrer now reports it (with its source line)
        // instead of letting it slip through to CodeGen.
        var source = "x -> 1\nz -> y";

        var diagnostics = DocumentAnalyzer.Analyze(source);

        diagnostics.Should().ContainSingle();
        diagnostics[0].Line.Should().Be(1); // 0-based -> second line
        diagnostics[0].Message.Should().Contain("'y' is not defined");
    }

    [Fact]
    public void Type_Error_Maps_To_Its_Line()
    {
        // Mixing int and string in arithmetic — the unifier reports a type mismatch
        // carrying the offending expression's source line.
        var diagnostics = DocumentAnalyzer.Analyze("x -> 1 + \"a\"");

        diagnostics.Should().ContainSingle();
        diagnostics[0].Line.Should().Be(0); // 0-based -> first line
        diagnostics[0].Message.Should().Contain("type mismatch");
    }

    [Fact]
    public void Type_Error_On_Later_Line_Maps_To_That_Line()
    {
        // The mismatch is on the third line; the diagnostic must land there, not on line 1.
        var source = "a -> 1\nb -> 2\nc -> a + \"x\"";

        var diagnostics = DocumentAnalyzer.Analyze(source);

        diagnostics.Should().ContainSingle();
        diagnostics[0].Line.Should().Be(2); // 0-based -> third line
        diagnostics[0].Message.Should().Contain("type mismatch");
    }
}
