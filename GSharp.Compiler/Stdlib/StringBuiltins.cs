using System.Globalization;
using System.Reflection;

namespace GSharp.Compiler.Stdlib;

public static class StringBuiltins
{
    public static void Register(Dictionary<string, MethodInfo> builtins)
    {
        builtins["string.from"] = typeof(StringBuiltins).GetMethod(nameof(From))!;
        builtins["string.len"] = typeof(StringBuiltins).GetMethod(nameof(Len))!;
        builtins["string.upper"] = typeof(StringBuiltins).GetMethod(nameof(Upper))!;
        builtins["string.lower"] = typeof(StringBuiltins).GetMethod(nameof(Lower))!;
        builtins["string.trim"] = typeof(StringBuiltins).GetMethod(nameof(Trim))!;
        builtins["string.contains"] = typeof(StringBuiltins).GetMethod(nameof(Contains))!;
        builtins["string.startsWith"] = typeof(StringBuiltins).GetMethod(nameof(StartsWith))!;
        builtins["string.endsWith"] = typeof(StringBuiltins).GetMethod(nameof(EndsWith))!;
        builtins["string.split"] = typeof(StringBuiltins).GetMethod(nameof(Split))!;
        builtins["string.replace"] = typeof(StringBuiltins).GetMethod(nameof(Replace))!;
        builtins["string.slice"] = typeof(StringBuiltins).GetMethod(nameof(Slice))!;
        builtins["string.toInt"] = typeof(StringBuiltins).GetMethod(nameof(ToInt))!;
        builtins["string.toFloat"] = typeof(StringBuiltins).GetMethod(nameof(ToFloat))!;
    }

    public static object From(object arg)
    {
        return arg?.ToString() ?? "";
    }

    public static object Len(object arg)
    {
        return ((string)arg).Length;
    }

    public static object Upper(object arg)
    {
        return ((string)arg).ToUpperInvariant();
    }

    public static object Lower(object arg)
    {
        return ((string)arg).ToLowerInvariant();
    }

    public static object Trim(object arg)
    {
        return ((string)arg).Trim();
    }

    public static object Contains(object arg, object substring)
    {
        return ((string)arg).Contains((string)substring, StringComparison.Ordinal);
    }

    public static object StartsWith(object arg, object prefix)
    {
        return ((string)arg).StartsWith((string)prefix, StringComparison.Ordinal);
    }

    public static object EndsWith(object arg, object suffix)
    {
        return ((string)arg).EndsWith((string)suffix, StringComparison.Ordinal);
    }

    public static object Split(object arg, object separator)
    {
        var parts = ((string)arg).Split((string)separator);
        var result = new object[parts.Length];
        for (var i = 0; i < parts.Length; i++)
            result[i] = parts[i];
        return result;
    }

    public static object Replace(object arg, object oldValue, object newValue)
    {
        return ((string)arg).Replace((string)oldValue, (string)newValue);
    }

    public static object Slice(object arg, object start, object end)
    {
        var s = (string)arg;
        var startIndex = (int)start;
        var endIndex = (int)end;
        return s[startIndex..endIndex];
    }

    public static object ToInt(object arg)
    {
        return int.Parse((string)arg, CultureInfo.InvariantCulture);
    }

    public static object ToFloat(object arg)
    {
        return float.Parse((string)arg, CultureInfo.InvariantCulture);
    }
}
