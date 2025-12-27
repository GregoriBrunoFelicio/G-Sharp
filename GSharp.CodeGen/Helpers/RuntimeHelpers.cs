namespace GSharp.CodeGen.Helpers;

public static class RuntimeHelpers
{
    public static object Add(object a, object b)
    {
        return a switch
        {
            int ai when b is int bi => ai + bi,
            double ad when b is double bd => ad + bd,
            string sa when b is string sb => sa + sb,
            _ => throw new Exception($"+ inválido: {a?.GetType()} e {b?.GetType()}")
        };
    }

    public static object Subtract(object a, object b)
    {
        return a switch
        {
            int ai when b is int bi => ai - bi,
            double ad when b is double bd => ad - bd,
            float af when b is float bf => af - bf,
            decimal am when b is decimal bm => am - bm,
            _ => throw Invalid(a, b, "-")
        };
    }

    public static object GreaterThan(object a, object b)
    {
        return a switch
        {
            int ai when b is int bi => ai > bi,
            double ad when b is double bd => ad > bd,
            _ => throw Invalid(a, b, ">")
        };
    }

    public static object LessThan(object a, object b)
    {
        return a switch
        {
            int ai when b is int bi => ai < bi,
            double ad when b is double bd => ad < bd,
            _ => throw Invalid(a, b, "<")
        };
    }

    public static object GreaterThanOrEqual(object a, object b)
    {
        return a switch
        {
            int ai when b is int bi => ai >= bi,
            double ad when b is double bd => ad >= bd,
            _ => throw Invalid(a, b, ">=")
        };
    }

    public static object LessThanOrEqual(object a, object b)
    {
        return a switch
        {
            int ai when b is int bi => ai <= bi,
            double ad when b is double bd => ad <= bd,
            _ => throw Invalid(a, b, "<=")
        };
    }

    public static object EqualEqual(object a, object b) => Equals(a, b);

    public static object NotEqual(object a, object b) => !Equals(a, b);

    public static bool IsTrue(object value)
    {
        if (value is bool b) return b;
        throw new Exception($"Condition is not boolean: {value?.GetType()}");
    }

    private static Exception Invalid(object a, object b, string op) =>
        new($"Operation '{op}' invalid between {a?.GetType()} and {b?.GetType()}");
}