using System.Text;
using GSharp.TypeChecker;

namespace GSharp.LanguageServer;

/// <summary>
/// Renders a <see cref="GsType"/> the way an editor tooltip should show it:
///   - curried function types flatten to <c>a → b → c</c> instead of nested parens;
///   - a function used as an argument keeps its parens, e.g. <c>(a → b) → c</c>;
///   - leftover type variables are renamed to <c>'a, 'b, …</c> in order of appearance,
///     so an unresolved generic reads cleanly instead of leaking inference ids like '?7.
///
/// This lives in the LSP layer on purpose: <see cref="GsType.ToString"/> is relied on by
/// the type checker's error messages and tests, so presentation tweaks stay out of it.
/// </summary>
public static class TypeDisplay
{
    public static string Format(GsType type)
    {
        var generics = new Dictionary<string, string>();
        return Render(type, generics);
    }

    private static string Render(GsType type, Dictionary<string, string> generics) => type switch
    {
        FunctionType function => RenderFunction(function, generics),
        ArrayType array       => $"[{Render(array.ElementType, generics)}]",
        TypeVar variable      => RenderTypeVar(variable, generics),
        _                     => type.ToString()
    };

    private static string RenderFunction(FunctionType function, Dictionary<string, string> generics)
    {
        var parts   = new List<string>();
        GsType node = function;

        // Walk the return spine so right-associated arrows collapse into one chain.
        while (node is FunctionType arrow)
        {
            parts.Add(RenderParameter(arrow.ParameterType, generics));
            node = arrow.ReturnType;
        }

        parts.Add(Render(node, generics));
        return string.Join(" → ", parts);
    }

    // A function-typed parameter needs parens to stay unambiguous; everything else doesn't.
    private static string RenderParameter(GsType parameter, Dictionary<string, string> generics) =>
        parameter is FunctionType ? $"({Render(parameter, generics)})" : Render(parameter, generics);

    private static string RenderTypeVar(TypeVar variable, Dictionary<string, string> generics)
    {
        if (!generics.TryGetValue(variable.Id, out var name))
        {
            name = NameForIndex(generics.Count);
            generics[variable.Id] = name;
        }
        return name;
    }

    // 0 → 'a, 1 → 'b, … 25 → 'z, 26 → 'a1, 27 → 'b1, …
    private static string NameForIndex(int index)
    {
        var letter  = (char)('a' + index % 26);
        var suffix  = index / 26;
        var builder = new StringBuilder("'").Append(letter);
        if (suffix > 0)
            builder.Append(suffix);
        return builder.ToString();
    }
}
