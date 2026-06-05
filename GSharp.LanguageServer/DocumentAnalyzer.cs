using System.Text.RegularExpressions;
using GSharp.TypeChecker;

namespace GSharp.LanguageServer;

/// <summary>
/// Runs the G# pipeline (Lexer → Parser → TypeInferrer) over an in-memory document
/// and turns any failure into diagnostics. LSP-agnostic on purpose, so it can be
/// unit-tested without spinning up a language server.
///
/// Phase 1 limitation: the pipeline aborts on the first error, so at most one
/// diagnostic is produced per analysis. Collecting multiple errors (and precise
/// columns) requires source spans on the AST — that is the hover/phase-2 foundation.
/// </summary>
public static class DocumentAnalyzer
{
    // Lexer and Parser prefix their error messages with the 1-based line, e.g. "5: unexpected 'x'".
    private static readonly Regex LinePrefix = new(@"^(\d+):\s*", RegexOptions.Compiled);

    /// <summary>Diagnostics only — kept for callers that don't need the typed AST.</summary>
    public static IReadOnlyList<AnalyzerDiagnostic> Analyze(string source) =>
        AnalyzeDocument(source).Diagnostics;

    /// <summary>
    /// Runs the full pipeline and returns diagnostics together with the parsed expressions
    /// and their inferred types, so the language server can answer hover requests.
    /// </summary>
    public static AnalysisResult AnalyzeDocument(string source)
    {
        // Empty or whitespace-only documents are valid (mirrors GsLoader.ParseFile and the
        // "allow empty .gs files" behaviour). The Lexer constructor throws on empty input,
        // so this must short-circuit before the pipeline runs.
        if (string.IsNullOrWhiteSpace(source))
            return AnalysisResult.Empty;

        try
        {
            var tokens      = new Lexer.Lexer(source).Tokenize();
            var expressions = new Parser.Parser(tokens).Parse();
            var types       = new TypeInferrer().Infer(expressions);
            return new AnalysisResult([], expressions, types);
        }
        catch (Exception exception)
        {
            return AnalysisResult.Empty with { Diagnostics = [ToDiagnostic(exception.Message)] };
        }
    }

    private static AnalyzerDiagnostic ToDiagnostic(string rawMessage)
    {
        var match = LinePrefix.Match(rawMessage);

        // Lexer/parser error — extract the line and drop the prefix from the shown message.
        if (match.Success && int.TryParse(match.Groups[1].Value, out var oneBasedLine) && oneBasedLine > 0)
        {
            var message = rawMessage[match.Length..];
            return new AnalyzerDiagnostic(oneBasedLine - 1, 0, message, AnalyzerSeverity.Error);
        }

        // Type errors (Unifier/TypeInferrer) and other failures carry no position yet.
        // Fall back to the first line until the AST gains source spans (phase 2).
        return new AnalyzerDiagnostic(0, 0, rawMessage, AnalyzerSeverity.Error);
    }
}
