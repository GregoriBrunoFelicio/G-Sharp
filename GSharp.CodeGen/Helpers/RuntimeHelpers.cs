namespace GSharp.CodeGen.Helpers;

public static class RuntimeHelpers
{
    // ===== ARITMÉTICA =====

    public static object Add(object a, object b)
    {
        if (a is int ai && b is int bi) return (object)(ai + bi);
        if (a is double ad && b is double bd) return (object)(ad + bd);
        if (a is string sa && b is string sb) return sa + sb;

        throw new Exception($"+ inválido: {a?.GetType()} e {b?.GetType()}");
    }
    
    public static object Subtract(object a, object b)
    {
        if (a is int ai && b is int bi)
            return (object)(ai - bi);

        if (a is double ad && b is double bd)
            return (object)(ad - bd);

        if (a is float af && b is float bf)
            return (object)(af - bf);

        if (a is decimal am && b is decimal bm)
            return (object)(am - bm);

        throw Invalid(a, b, "-");
    }

    // ===== COMPARAÇÕES =====

    public static object GreaterThan(object a, object b)
    {
        if (a is int ai && b is int bi) return (object)(ai > bi);
        if (a is double ad && b is double bd) return (object)(ad > bd);
        throw Invalid(a, b, ">");
    }

    public static object LessThan(object a, object b)
    {
        if (a is int ai && b is int bi) return (object)(ai < bi);
        if (a is double ad && b is double bd) return (object)(ad < bd);
        throw Invalid(a, b, "<");
    }

    public static object GreaterThanOrEqual(object a, object b)
    {
        if (a is int ai && b is int bi) return (object)(ai >= bi);
        if (a is double ad && b is double bd) return (object)(ad >= bd);
        throw Invalid(a, b, ">=");
    }

    public static object LessThanOrEqual(object a, object b)
    {
        if (a is int ai && b is int bi) return (object)(ai <= bi);
        if (a is double ad && b is double bd) return (object)(ad <= bd);
        throw Invalid(a, b, "<=");
    }

    public static object EqualEqual(object a, object b)
    {
        return (object)Equals(a, b);
    }

    public static object NotEqual(object a, object b)
    {
        return (object)!Equals(a, b);
    }

    // ===== CONTROLE DE FLUXO =====

    public static bool IsTrue(object value)
    {
        if (value is bool b) return b;
        throw new Exception($"Condição não é booleana: {value?.GetType()}");
    }

    private static Exception Invalid(object a, object b, string op) =>
        new Exception($"Operação '{op}' inválida entre {a?.GetType()} e {b?.GetType()}");
}
