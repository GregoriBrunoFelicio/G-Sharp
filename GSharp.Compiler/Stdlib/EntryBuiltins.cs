using System.Reflection;
using GSharp.Compiler.CodeGen.Helpers;

namespace GSharp.Compiler.Stdlib;

public static class EntryBuiltins
{
    public static void Register(Dictionary<string, MethodInfo> builtins)
    {
        builtins["entry.key"] = typeof(EntryBuiltins).GetMethod(nameof(Key))!;
        builtins["entry.value"] = typeof(EntryBuiltins).GetMethod(nameof(Value))!;
    }

    public static object Key(object entry)
    {
        return ((GsEntry)entry).Key;
    }

    public static object Value(object entry)
    {
        return ((GsEntry)entry).Value;
    }
}
