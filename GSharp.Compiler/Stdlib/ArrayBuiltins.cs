using System.Reflection;
using GSharp.Compiler.CodeGen.Helpers;

namespace GSharp.Compiler.Stdlib;

public static class ArrayBuiltins
{
    public static void Register(Dictionary<string, MethodInfo> builtins)
    {
        builtins["array.head"] = typeof(ArrayBuiltins).GetMethod(nameof(Head))!;
        builtins["array.tail"] = typeof(ArrayBuiltins).GetMethod(nameof(Tail))!;
        builtins["array.last"] = typeof(ArrayBuiltins).GetMethod(nameof(Last))!;
        builtins["array.len"] = typeof(ArrayBuiltins).GetMethod(nameof(Len))!;
        builtins["array.empty"] = typeof(ArrayBuiltins).GetMethod(nameof(Empty))!;
        builtins["array.reverse"] = typeof(ArrayBuiltins).GetMethod(nameof(Reverse))!;
        builtins["array.concat"] = typeof(ArrayBuiltins).GetMethod(nameof(Concat))!;
        builtins["array.sort"] = typeof(ArrayBuiltins).GetMethod(nameof(Sort))!;
        builtins["array.take"] = typeof(ArrayBuiltins).GetMethod(nameof(Take))!;
        builtins["array.map"] = typeof(ArrayBuiltins).GetMethod(nameof(Map))!;
        builtins["array.filter"] = typeof(ArrayBuiltins).GetMethod(nameof(Filter))!;
        builtins["array.fold"] = typeof(ArrayBuiltins).GetMethod(nameof(Fold))!;
    }

    public static object Map(object arg, object fn)
    {
        var arr = (object[])arg;
        var f = (GSharpFunction)fn;
        var result = new object[arr.Length];
        for (var i = 0; i < arr.Length; i++)
            result[i] = f.Call1(arr[i]);
        return result;
    }

    public static object Filter(object arg, object fn)
    {
        var arr = (object[])arg;
        var f = (GSharpFunction)fn;
        var matches = new List<object>();
        foreach (var element in arr)
            if ((bool)f.Call1(element))
                matches.Add(element);
        return matches.ToArray();
    }

    public static object Fold(object arg, object seed, object fn)
    {
        var arr = (object[])arg;
        var f = (GSharpFunction)fn;
        var accumulator = seed;
        foreach (var element in arr)
            accumulator = f.Call([accumulator, element]);
        return accumulator;
    }

    public static object Take(object arg, object quantity)
    {
        var arr = (object[])arg;
        if (arr.Length == 0) throw new Exception("array.head: empty array");
        var result = new object[(int)quantity];
        Array.Copy(arr, 0, result, 0, (int)quantity);
        return result;
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

    public static object Len(object arg)
    {
        return ((object[])arg).Length;
    }

    public static object Empty(object arg)
    {
        return ((object[])arg).Length == 0;
    }

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