namespace GSharp.Stdlib;

public static class BuiltinCatalog
{
    private static readonly IReadOnlyDictionary<string, int> Array = new Dictionary<string, int>
    {
        { "array.head", 1 },
        { "array.tail", 1 },
        { "array.last", 1 },
        { "array.len", 1 },
        { "array.empty", 1 },
        { "array.reverse", 1 },
        { "array.concat", 2 },
        { "array.sort", 1 },
    };

    private static readonly IReadOnlyDictionary<string, int> String = new Dictionary<string, int>
    {
        { "string.from", 1 },
    };

    public static IReadOnlyDictionary<string, int> All =>
        Array.Concat(String).ToDictionary(p => p.Key, p => p.Value);
}
