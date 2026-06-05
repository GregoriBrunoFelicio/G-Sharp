using GSharp.AST;
using GSharp.TypeChecker;

namespace GSharp.LanguageServer;

/// <summary>
/// Everything one analysis pass produces: the diagnostics to publish, plus the parsed
/// expressions and their inferred types. The latter two power hover — they let the
/// <see cref="HoverHandler"/> map a cursor position back to an expression and its type.
///
/// On a lexer/parser/type error the pipeline aborts early, so <see cref="Expressions"/>
/// and <see cref="Types"/> come back empty while <see cref="Diagnostics"/> carries the error.
/// </summary>
public record AnalysisResult(
    IReadOnlyList<AnalyzerDiagnostic> Diagnostics,
    IReadOnlyList<Expression> Expressions,
    IReadOnlyDictionary<Expression, GsType> Types)
{
    public static readonly AnalysisResult Empty =
        new([], [], new Dictionary<Expression, GsType>());
}
