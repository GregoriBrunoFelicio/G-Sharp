using System.Reflection;

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
