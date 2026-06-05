using GSharp.AST;
using GSharp.TypeChecker;

namespace GSharp.LanguageServer;

/// <summary>The text and 1-based source range to show for a hover (LSP-agnostic for testing).</summary>
public record HoverResult(string Markdown, int Line, int StartColumn, int EndColumn);

/// <summary>
/// Answers "what is under the cursor?" by walking the typed AST.
///
/// All coordinates here are 1-based, matching the lexer's <see cref="GSharp.Lexer.Token"/>
/// positions stored on each <see cref="Expression"/>. The caller converts to/from LSP's
/// 0-based positions.
///
/// The cursor lands on the innermost named construct whose source range contains it, and
/// the tooltip shows that name with its inferred type — e.g. <c>add : int → int → int</c>
/// for a function, <c>x : int</c> for a binding. Function call sites show the callee's full
/// </summary>
public static class HoverProvider
{
    public static HoverResult? Find(AnalysisResult analysis, int line, int character)
    {
        var signatures = CollectFunctionSignatures(analysis);

        Candidate? best = null;
        foreach (var expression in analysis.Expressions)
            foreach (var node in Walk(expression))
            {
                var candidate = ToCandidate(node, analysis.Types, signatures);
                if (candidate is null)
                    continue;

                if (!candidate.Contains(line, character))
                    continue;

                // Prefer the tightest span so a literal wins over the call that encloses it.
                if (best is null || candidate.Width < best.Width)
                    best = candidate;
            }

        if (best is null)
            return null;

        var markdown = $"```gsharp\n{best.Label}\n```";
        return new HoverResult(markdown, best.Line, best.StartColumn, best.StartColumn + best.Width);
    }

    // Maps each top-level function name to its inferred (curried) signature, so a call site
    // can show the whole signature rather than the call's result type.
    private static Dictionary<string, GsType> CollectFunctionSignatures(AnalysisResult analysis)
    {
        var signatures = new Dictionary<string, GsType>();
        foreach (var expression in analysis.Expressions)
            if (expression is FunctionDeclaration fn && analysis.Types.TryGetValue(fn, out var type))
                signatures[fn.Name] = type;
        return signatures;
    }

    private static Candidate? ToCandidate(
        Expression node,
        IReadOnlyDictionary<Expression, GsType> types,
        IReadOnlyDictionary<string, GsType> signatures)
    {
        if (node.Line <= 0)
            return null;

        switch (node)
        {
            case FunctionDeclaration fn when types.TryGetValue(fn, out var fnType):
                return Named(fn.Name, fnType, node);

            case LetExpression let when types.TryGetValue(let, out var letType):
                return Named(let.BindingName, letType, node);

            case CallExpression call:
                // Prefer the callee's signature; fall back to the call's result type
                // (e.g. precompiled built-ins, which have no FunctionDeclaration).
                var callType = signatures.TryGetValue(call.Callee, out var signature)
                    ? signature
                    : types.GetValueOrDefault(call);
                return callType is null ? null : Named(call.Callee, callType, node, call.Callee.Length);

            case QualifiedCallExpression qualified when types.TryGetValue(qualified, out var qualifiedType):
                var qualifiedName = $"{qualified.Module}.{qualified.Function}";
                return Named(qualifiedName, qualifiedType, node, qualifiedName.Length);

            case BindingExpression binding when types.TryGetValue(binding, out var bindingType):
                return Named(binding.Name, bindingType, node, binding.Name.Length);

            case LiteralExpression literal when types.TryGetValue(literal, out var literalType):
                // Literals have no name — show just the type. Width is approximate.
                return new Candidate(node.Line, node.Column, LiteralWidth(literal), TypeDisplay.Format(literalType));

            default:
                return null;
        }
    }

    private static Candidate Named(string name, GsType type, Expression node) =>
        Named(name, type, node, name.Length);

    private static Candidate Named(string name, GsType type, Expression node, int width) =>
        new(node.Line, node.Column, Math.Max(1, width), $"{name} : {TypeDisplay.Format(type)}");

    private static int LiteralWidth(LiteralExpression literal) => literal.Value switch
    {
        string text => text.Length + 2, // the source span includes the surrounding quotes
        null        => 1,
        object[]    => 1,               // arrays span multiple tokens; don't guess
        var value   => value.ToString()?.Length ?? 1
    };

    private static IEnumerable<Expression> Walk(Expression expression)
    {
        yield return expression;

        switch (expression)
        {
            case BinaryExpression binary:
                foreach (var child in Walk(binary.Left)) yield return child;
                foreach (var child in Walk(binary.Right)) yield return child;
                break;

            case LetExpression let:
                foreach (var child in Walk(let.Value)) yield return child;
                break;

            case PrintExpression print:
                foreach (var child in Walk(print.Value)) yield return child;
                break;

            case IfExpression ifExpression:
                foreach (var child in Walk(ifExpression.Condition)) yield return child;
                foreach (var child in WalkAll(ifExpression.ThenBody)) yield return child;
                if (ifExpression.ElseBody is not null)
                    foreach (var child in WalkAll(ifExpression.ElseBody)) yield return child;
                break;

            case ForExpression forExpression:
                foreach (var child in Walk(forExpression.Iterable)) yield return child;
                foreach (var child in WalkAll(forExpression.Body)) yield return child;
                break;

            case FunctionDeclaration fn:
                foreach (var child in WalkAll(fn.Body)) yield return child;
                break;

            case CallExpression call:
                foreach (var child in WalkAll(call.Arguments)) yield return child;
                break;

            case QualifiedCallExpression qualified:
                foreach (var child in WalkAll(qualified.Arguments)) yield return child;
                break;
        }
    }

    private static IEnumerable<Expression> WalkAll(IEnumerable<Expression> expressions)
    {
        foreach (var expression in expressions)
            foreach (var child in Walk(expression))
                yield return child;
    }

    private record Candidate(int Line, int StartColumn, int Width, string Label)
    {
        public bool Contains(int line, int character) =>
            line == Line && character >= StartColumn && character < StartColumn + Width;
    }
}
