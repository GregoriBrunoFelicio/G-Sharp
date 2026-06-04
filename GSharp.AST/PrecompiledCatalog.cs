namespace GSharp.AST;

public static class PrecompiledCatalog
{
    public static readonly IReadOnlyDictionary<string, int> Functions = new Dictionary<string, int>
    {
        { "head",    1 },
        { "tail",    1 },
        { "last",    1 },
        { "len",     1 },
        { "empty",   1 },
        { "nth",     2 },
        { "reverse", 1 },
        { "concat",  2 },
        { "str",     1 },
    };
}
