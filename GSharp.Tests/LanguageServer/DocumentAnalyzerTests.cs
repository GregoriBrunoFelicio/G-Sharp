using FluentAssertions;
using GSharp.LanguageServer;

namespace G.Sharp.Compiler.Tests.LanguageServer;

public class DocumentAnalyzerTests
{
    [Fact]
    public void Valid_Source_Produces_No_Diagnostics()
    {
        var diagnostics = DocumentAnalyzer.Analyze("let x = 42");

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
        var source = "let x = 42\n@";

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
            "    let a = 1\n" +
            "  let b = 2\n" +
            "        let c = 3\n";

        var diagnostics = DocumentAnalyzer.Analyze(source);

        diagnostics.Should().ContainSingle();
        diagnostics[0].Line.Should().BeGreaterThan(0); // not the line-1 fallback
    }

    [Fact]
    public void Type_Error_Falls_Back_To_First_Line()
    {
        // Mixing int and string in arithmetic — the unifier reports a type mismatch
        // with no source position, so it lands on the first line.
        var diagnostics = DocumentAnalyzer.Analyze("let x = 1 + \"a\"");

        diagnostics.Should().ContainSingle();
        diagnostics[0].Line.Should().Be(0);
        diagnostics[0].Message.Should().Contain("type mismatch");
    }
}
