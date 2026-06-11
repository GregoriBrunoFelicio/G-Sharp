using System.Reflection;

namespace GSharp.Stdlib;

public static class StringBuiltins
{
    public static void Register(Dictionary<string, MethodInfo> builtins)
    {
        builtins["string.from"] = typeof(StringBuiltins).GetMethod(nameof(From))!;
    }

    public static object From(object arg) => arg?.ToString() ?? "";
}
