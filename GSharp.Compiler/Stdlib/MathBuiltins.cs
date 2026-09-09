using System.Reflection;

namespace GSharp.Compiler.Stdlib;

public static class MathBuiltins
{
    public static void Register(Dictionary<string, MethodInfo> builtins)
    {
        builtins["math.abs"] = typeof(MathBuiltins).GetMethod(nameof(Abs))!;
        builtins["math.floor"] = typeof(MathBuiltins).GetMethod(nameof(Floor))!;
        builtins["math.ceil"] = typeof(MathBuiltins).GetMethod(nameof(Ceil))!;
        builtins["math.round"] = typeof(MathBuiltins).GetMethod(nameof(Round))!;
        builtins["math.sqrt"] = typeof(MathBuiltins).GetMethod(nameof(Sqrt))!;
        builtins["math.pow"] = typeof(MathBuiltins).GetMethod(nameof(Pow))!;
        builtins["math.min"] = typeof(MathBuiltins).GetMethod(nameof(Min))!;
        builtins["math.max"] = typeof(MathBuiltins).GetMethod(nameof(Max))!;
        builtins["math.mod"] = typeof(MathBuiltins).GetMethod(nameof(Mod))!;
        builtins["math.pi"] = typeof(MathBuiltins).GetMethod(nameof(Pi))!;
        builtins["math.e"] = typeof(MathBuiltins).GetMethod(nameof(E))!;
    }

    // abs/floor/ceil/round preserve the operand's numeric type (int/float/double/decimal),
    // mirroring how unary `-` and binary arithmetic behave elsewhere in the language —
    // see TypeInferrer.Builtins.cs, where the argument and return type share one fresh TypeVar.
    public static object Abs(object arg)
    {
        return arg switch
        {
            int i => Math.Abs(i),
            float f => Math.Abs(f),
            double d => Math.Abs(d),
            decimal m => Math.Abs(m),
            _ => throw new Exception($"math.abs: unsupported type {arg.GetType()}")
        };
    }

    public static object Floor(object arg)
    {
        return arg switch
        {
            int i => i,
            float f => MathF.Floor(f),
            double d => Math.Floor(d),
            decimal m => Math.Floor(m),
            _ => throw new Exception($"math.floor: unsupported type {arg.GetType()}")
        };
    }

    public static object Ceil(object arg)
    {
        return arg switch
        {
            int i => i,
            float f => MathF.Ceiling(f),
            double d => Math.Ceiling(d),
            decimal m => Math.Ceiling(m),
            _ => throw new Exception($"math.ceil: unsupported type {arg.GetType()}")
        };
    }

    public static object Round(object arg)
    {
        return arg switch
        {
            int i => i,
            float f => MathF.Round(f),
            double d => Math.Round(d),
            decimal m => Math.Round(m),
            _ => throw new Exception($"math.round: unsupported type {arg.GetType()}")
        };
    }

    // sqrt/pow always return double regardless of operand type — an irrational result can't
    // stay in the operand's original numeric type, unlike abs/floor/ceil/round above.
    public static object Sqrt(object arg)
    {
        return Math.Sqrt(Convert.ToDouble(arg));
    }

    public static object Pow(object baseValue, object exponent)
    {
        return Math.Pow(Convert.ToDouble(baseValue), Convert.ToDouble(exponent));
    }

    public static object Min(object a, object b)
    {
        return ((IComparable)a).CompareTo(b) <= 0 ? a : b;
    }

    public static object Max(object a, object b)
    {
        return ((IComparable)a).CompareTo(b) >= 0 ? a : b;
    }

    public static object Mod(object a, object b)
    {
        return a switch
        {
            int i => i % (int)b,
            float f => f % (float)b,
            double d => d % (double)b,
            decimal m => m % (decimal)b,
            _ => throw new Exception($"math.mod: unsupported type {a.GetType()}")
        };
    }

    public static object Pi()
    {
        return Math.PI;
    }

    public static object E()
    {
        return Math.E;
    }
}
