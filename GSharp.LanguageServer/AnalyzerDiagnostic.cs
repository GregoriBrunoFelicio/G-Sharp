namespace GSharp.LanguageServer;

/// <summary>How serious a diagnostic is. Phase 1 only ever produces <see cref="Error"/>.</summary>
public enum AnalyzerSeverity
{
    Error,
    Warning,
    Information,
    Hint
}

/// <summary>
/// An editor-agnostic diagnostic. Positions are 0-based (LSP convention) so the
/// handler can map straight onto an LSP Range without further arithmetic.
/// G# itself reports 1-based lines, so <see cref="DocumentAnalyzer"/> converts.
/// </summary>
public record AnalyzerDiagnostic(int Line, int Column, string Message, AnalyzerSeverity Severity);
