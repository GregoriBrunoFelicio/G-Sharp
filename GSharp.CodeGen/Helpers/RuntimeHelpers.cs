namespace GSharp.CodeGen.Helpers;

/// <summary>
/// Provides runtime support for dynamic operations in G#.
/// </summary>
public static class RuntimeHelpers
{
    /// <summary>
    /// Promotes two numeric values to a common type before an operation.
    /// Hierarchy: int &lt; float &lt; double &lt; decimal.
    /// </summary>
    private static (object a, object b) Promote(object a, object b)
    {
        if (a.GetType() == b.GetType()) return (a, b);

        return (a, b) switch
        {
            (int ai,      float)    => ((float)ai,           b),
            (float,       int bi)   => (a,                   (float)bi),
            (int ai,      double)   => ((double)ai,          b),
            (double,      int bi)   => (a,                   (double)bi),
            (int ai,      decimal)  => ((decimal)ai,         b),
            (decimal,     int bi)   => (a,                   (decimal)bi),
            (float af,    double)   => ((double)af,          b),
            (double,      float bf) => (a,                   (double)bf),
            (float af,    decimal)  => ((decimal)(double)af, b),
            (decimal,     float bf) => (a,                   (decimal)(double)bf),
            (double ad,   decimal)  => ((decimal)ad,         b),
            (decimal,     double bd)=> (a,                   (decimal)bd),
            _ => throw new Exception(
                $"Incompatible types: {a.GetType().Name} and {b.GetType().Name}")
        };
    }

    private static bool IsNumeric(object v) =>
        v is int or float or double or decimal;

    /// <summary>
    /// Adds two values. Supports numeric promotion and string concatenation.
    /// </summary>
    public static object Add(object a, object b)
    {
        if (a is string sa && b is string sb) return sa + sb;

        if (!IsNumeric(a) || !IsNumeric(b))
            throw Invalid(a, b, "+");

        var (pa, pb) = Promote(a, b);
        return pa switch
        {
            int     ai => ai + (int)pb,
            float   af => af + (float)pb,
            double  ad => ad + (double)pb,
            decimal am => am + (decimal)pb,
            _ => throw Invalid(a, b, "+")
        };
    }

    public static object Subtract(object a, object b)
    {
        if (!IsNumeric(a) || !IsNumeric(b))
            throw Invalid(a, b, "-");

        var (pa, pb) = Promote(a, b);
        return pa switch
        {
            int     ai => ai - (int)pb,
            float   af => af - (float)pb,
            double  ad => ad - (double)pb,
            decimal am => am - (decimal)pb,
            _ => throw Invalid(a, b, "-")
        };
    }

    public static object Multiply(object a, object b)
    {
        if (!IsNumeric(a) || !IsNumeric(b))
            throw Invalid(a, b, "*");

        var (pa, pb) = Promote(a, b);
        return pa switch
        {
            int     ai => ai * (int)pb,
            float   af => af * (float)pb,
            double  ad => ad * (double)pb,
            decimal am => am * (decimal)pb,
            _ => throw Invalid(a, b, "*")
        };
    }

    /// <summary>
    /// Divides two values. int and decimal throw on zero;
    /// float and double follow IEEE 754.
    /// </summary>
    public static object Divide(object a, object b)
    {
        if (!IsNumeric(a) || !IsNumeric(b))
            throw Invalid(a, b, "/");

        var (pa, pb) = Promote(a, b);
        return pa switch
        {
            int     ai when (int)pb     == 0  => throw new DivideByZeroException(),
            decimal am when (decimal)pb == 0m => throw new DivideByZeroException(),
            int     ai => ai / (int)pb,
            float   af => af / (float)pb,
            double  ad => ad / (double)pb,
            decimal am => am / (decimal)pb,
            _ => throw Invalid(a, b, "/")
        };
    }

    public static object GreaterThan(object a, object b)        => Compare(a, b, ">")  > 0;
    public static object LessThan(object a, object b)           => Compare(a, b, "<")  < 0;
    public static object GreaterThanOrEqual(object a, object b) => Compare(a, b, ">=") >= 0;
    public static object LessThanOrEqual(object a, object b)    => Compare(a, b, "<=") <= 0;

    public static object EqualEqual(object a, object b) => Equals(a, b);
    public static object NotEqual(object a, object b)   => !Equals(a, b);

    /// <summary>
    /// Evaluates a runtime value as a boolean condition.
    /// Only boolean expressions are valid as conditions in G#.
    /// </summary>
    public static bool IsTrue(object value) => value switch
    {
        bool b => b,
        _ => throw new Exception($"Condition is not boolean: {value?.GetType().Name}")
    };

    /// <summary>
    /// Compares two numeric values after promotion.
    /// Follows the <see cref="IComparable"/> contract.
    /// </summary>
    private static int Compare(object a, object b, string op)
    {
        if (!IsNumeric(a) || !IsNumeric(b))
            throw Invalid(a, b, op);

        var (pa, pb) = Promote(a, b);
        return pa switch
        {
            int     ai => ai.CompareTo((int)pb),
            float   af => af.CompareTo((float)pb),
            double  ad => ad.CompareTo((double)pb),
            decimal am => am.CompareTo((decimal)pb),
            _ => throw Invalid(a, b, op)
        };
    }

    private static Exception Invalid(object a, object b, string op) =>
        new($"Invalid operation '{op}' between {a?.GetType().Name} and {b?.GetType().Name}");
}