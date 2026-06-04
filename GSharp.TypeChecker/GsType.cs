namespace GSharp.TypeChecker;

/// <summary>Base type for all G# types.</summary>
public abstract record GsType
{
    public abstract override string ToString();
}

/// <summary>32-bit integer (e.g. 42).</summary>
public record IntType : GsType
{
    public override string ToString() => "int";
}

/// <summary>Single-precision float (e.g. 2.5f).</summary>
public record FloatType : GsType
{
    public override string ToString() => "float";
}

/// <summary>Double-precision float (e.g. 3.14d).</summary>
public record DoubleType : GsType
{
    public override string ToString() => "double";
}

/// <summary>Decimal (e.g. 9.99m).</summary>
public record DecimalType : GsType
{
    public override string ToString() => "decimal";
}

/// <summary>String (e.g. "hello").</summary>
public record StringType : GsType
{
    public override string ToString() => "string";
}

/// <summary>Boolean (true or false).</summary>
public record BoolType : GsType
{
    public override string ToString() => "bool";
}

/// <summary>
/// Unit — absence of a meaningful value.
/// Returned by println, while, let, and similar side-effecting expressions.
/// </summary>
public record UnitType : GsType
{
    public override string ToString() => "unit";
}

/// <summary>Homogeneous array whose elements all share the same type.</summary>
public record ArrayType(GsType ElementType) : GsType
{
    public override string ToString() => $"[{ElementType}]";
}

/// <summary>
/// Single-argument function type.
/// G# functions are curried — add a b : int → int → int is represented as
/// FunctionType(int, FunctionType(int, int)).
/// </summary>
public record FunctionType(GsType ParameterType, GsType ReturnType) : GsType
{
    public override string ToString() => $"({ParameterType} → {ReturnType})";
}

/// <summary>
/// Type variable — a placeholder used during inference when the concrete type is not yet known.
/// The Unifier resolves these to concrete types by solving the accumulated constraints.
/// </summary>
public record TypeVar(string Id) : GsType
{
    public override string ToString() => $"'{Id}";
}
