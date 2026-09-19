namespace GSharp.Compiler.TypeChecker;

/// <summary>
///     Maps `receiver.member` onto the stdlib builtin it stands for (e.g. a date receiver and
///     `year` → "time.year"). Shared by the type checker and codegen so both pick the same builtin:
///     the type checker calls it with the receiver type known so far, codegen with the final one.
/// </summary>
public static class MemberResolver
{
    public static string Resolve(GsType? receiverType, string member, IEnumerable<string> builtinNames, int line)
    {
        var position = line > 0 ? $"{line}: " : "";
        var names = builtinNames as ICollection<string> ?? builtinNames.ToList();

        var module = receiverType switch
        {
            DateType => "time",
            StringType => "string",
            ArrayType => "array",
            MapType => "map",
            IntType or FloatType or DoubleType or DecimalType => "math",
            _ => null
        };

        if (module is not null)
        {
            var qualifiedName = $"{module}.{member}";
            if (names.Contains(qualifiedName))
                return qualifiedName;
            throw new Exception($"{position}'{receiverType}' has no member '{member}'");
        }

        // Receiver type not known yet (e.g. a function parameter) — only a member name that
        // exists in exactly one module can be resolved without it.
        if (receiverType is not null && receiverType is not TypeVar)
            throw new Exception($"{position}'{receiverType}' has no member '{member}'");

        var candidates = names.Where(name => name.EndsWith("." + member)).OrderBy(name => name).ToList();
        if (candidates.Count == 1)
            return candidates[0];

        if (candidates.Count == 0)
            throw new Exception($"{position}unknown member '{member}'");

        throw new Exception(
            $"{position}cannot resolve '.{member}' — the receiver's type is not known here; call it as " +
            string.Join(" or ", candidates));
    }
}
