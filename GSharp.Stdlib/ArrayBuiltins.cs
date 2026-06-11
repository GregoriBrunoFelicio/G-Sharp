using System.Reflection;

namespace GSharp.Stdlib;

public static class ArrayBuiltins
{
    public static void Register(Dictionary<string, MethodInfo> builtins)
    {
        builtins["array.head"]    = typeof(ArrayBuiltins).GetMethod(nameof(Head))!;
        builtins["array.tail"]    = typeof(ArrayBuiltins).GetMethod(nameof(Tail))!;
        builtins["array.last"]    = typeof(ArrayBuiltins).GetMethod(nameof(Last))!;
        builtins["array.len"]     = typeof(ArrayBuiltins).GetMethod(nameof(Len))!;
        builtins["array.empty"]   = typeof(ArrayBuiltins).GetMethod(nameof(Empty))!;
        builtins["array.reverse"] = typeof(ArrayBuiltins).GetMethod(nameof(Reverse))!;
        builtins["array.concat"]  = typeof(ArrayBuiltins).GetMethod(nameof(Concat))!;
        builtins["array.sort"]    = typeof(ArrayBuiltins).GetMethod(nameof(Sort))!;
    }

    public static object Head(object arg)
    {
        var arr = (object[])arg;
        if (arr.Length == 0) throw new Exception("array.head: empty array");
        return arr[0];
    }

    public static object Tail(object arg)
    {
        var arr = (object[])arg;
        if (arr.Length == 0) throw new Exception("array.tail: empty array");
        var result = new object[arr.Length - 1];
        Array.Copy(arr, 1, result, 0, result.Length);
        return result;
    }

    public static object Last(object arg)
    {
        var arr = (object[])arg;
        if (arr.Length == 0) throw new Exception("array.last: empty array");
        return arr[^1];
    }

    public static object Len(object arg) => ((object[])arg).Length;

    public static object Empty(object arg) => ((object[])arg).Length == 0;

    public static object Reverse(object arg)
    {
        var arr = (object[])arg;
        var result = new object[arr.Length];
        Array.Copy(arr, result, arr.Length);
        Array.Reverse(result);
        return result;
    }

    public static object Concat(object a, object b)
    {
        var left = a as object[] ?? [a];
        var right = b as object[] ?? [b];
        var merged = new object[left.Length + right.Length];
        Array.Copy(left, merged, left.Length);
        Array.Copy(right, 0, merged, left.Length, right.Length);
        return merged;
    }

    public static object Sort(object arg)
    {
        var arr = (object[])arg;
        var result = new object[arr.Length];
        Array.Copy(arr, result, arr.Length);
        Array.Sort(result);
        return result;
    }
}
