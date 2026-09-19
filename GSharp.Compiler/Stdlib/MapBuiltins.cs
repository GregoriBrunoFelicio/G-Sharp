using System.Reflection;
using GSharp.Compiler.CodeGen.Helpers;

namespace GSharp.Compiler.Stdlib;

public static class MapBuiltins
{
    public static void Register(Dictionary<string, MethodInfo> builtins)
    {
        builtins["map.empty"] = typeof(MapBuiltins).GetMethod(nameof(Empty))!;
        builtins["map.set"] = typeof(MapBuiltins).GetMethod(nameof(Set))!;
        builtins["map.get"] = typeof(MapBuiltins).GetMethod(nameof(Get))!;
        builtins["map.has"] = typeof(MapBuiltins).GetMethod(nameof(Has))!;
        builtins["map.remove"] = typeof(MapBuiltins).GetMethod(nameof(Remove))!;
        builtins["map.keys"] = typeof(MapBuiltins).GetMethod(nameof(Keys))!;
        builtins["map.values"] = typeof(MapBuiltins).GetMethod(nameof(Values))!;
        builtins["map.size"] = typeof(MapBuiltins).GetMethod(nameof(Size))!;
        builtins["map.getOr"] = typeof(MapBuiltins).GetMethod(nameof(GetOr))!;
        builtins["map.entries"] = typeof(MapBuiltins).GetMethod(nameof(Entries))!;
    }

    public static object Empty()
    {
        return new Dictionary<object, object>();
    }

    // Never mutates the input map — copies, mutates the copy, returns it. Same
    // immutability contract as every G# binding.
    public static object Set(object map, object key, object value)
    {
        var copy = new Dictionary<object, object>((Dictionary<object, object>)map) { [key] = value };
        return copy;
    }

    public static object Get(object map, object key)
    {
        return ((Dictionary<object, object>)map)[key];
    }

    // The value for `key`, or `fallback` when the key is absent — the non-throwing lookup.
    public static object GetOr(object map, object key, object fallback)
    {
        return ((Dictionary<object, object>)map).TryGetValue(key, out var value) ? value : fallback;
    }

    // Same order as a `for` loop over the map (see RuntimeHelpers.AsIterable).
    public static object Entries(object map)
    {
        return RuntimeHelpers.AsIterable(map);
    }

    public static object Has(object map, object key)
    {
        return ((Dictionary<object, object>)map).ContainsKey(key);
    }

    public static object Remove(object map, object key)
    {
        var copy = new Dictionary<object, object>((Dictionary<object, object>)map);
        copy.Remove(key);
        return copy;
    }

    public static object Keys(object map)
    {
        return ((Dictionary<object, object>)map).Keys.ToArray();
    }

    public static object Values(object map)
    {
        return ((Dictionary<object, object>)map).Values.ToArray();
    }

    public static object Size(object map)
    {
        return ((Dictionary<object, object>)map).Count;
    }
}
