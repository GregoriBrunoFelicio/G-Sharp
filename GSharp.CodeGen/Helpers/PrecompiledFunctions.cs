namespace GSharp.CodeGen.Helpers;

public static class PrecompiledFunctions
{
    public static object Head(object arg)
    {
        var arr = (object[])arg;
        if (arr.Length == 0) throw new Exception("head: empty array");
        return arr[0];
    }

    public static object Tail(object arg)
    {
        var arr = (object[])arg;
        if (arr.Length == 0) throw new Exception("tail: empty array");
        var result = new object[arr.Length - 1];
        Array.Copy(arr, 1, result, 0, result.Length);
        return result;
    }

    public static object Last(object arg)
    {
        var arr = (object[])arg;
        if (arr.Length == 0) throw new Exception("last: empty array");
        return arr[^1];
    }

    public static object Len(object arg) => ((object[])arg).Length;
    public static object Empty(object arg) => ((object[])arg).Length == 0;

    public static object Nth(object arrArg, object idxArg)
    {
        var arr = (object[])arrArg;
        var idx = Convert.ToInt32(idxArg);
        if (idx < 0 || idx >= arr.Length)
            throw new Exception($"nth: index {idx} out of bounds (length {arr.Length})");
        return arr[idx];
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
        Array.Sort(arr);
        return arr;
    }

    public static object Str(object arg) => arg?.ToString() ?? "";
}
