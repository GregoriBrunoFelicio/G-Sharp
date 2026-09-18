using System.Globalization;
using System.Reflection;

namespace GSharp.Compiler.Stdlib;

public static class IOBuiltins
{
    public static void Register(Dictionary<string, MethodInfo> builtins)
    {
        builtins["io.readLine"] = typeof(IOBuiltins).GetMethod(nameof(ReadLine))!;
        builtins["io.readInt"] = typeof(IOBuiltins).GetMethod(nameof(ReadInt))!;
        builtins["io.readFloat"] = typeof(IOBuiltins).GetMethod(nameof(ReadFloat))!;
    }

    // Console.ReadLine() returns null at EOF; GsType.StringType has no notion of
    // null, so EOF normalizes to an empty string.
    public static object ReadLine()
    {
        return Console.ReadLine() ?? "";
    }

    // Same "no try-parse" philosophy as string.toInt/toFloat: malformed input throws
    // an unhandled FormatException rather than being wrapped in a Result-like type.
    public static object ReadInt()
    {
        return int.Parse((string)ReadLine(), CultureInfo.InvariantCulture);
    }

    public static object ReadFloat()
    {
        return float.Parse((string)ReadLine(), CultureInfo.InvariantCulture);
    }
}
