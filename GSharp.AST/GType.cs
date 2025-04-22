namespace GSharp.AST;

public enum GPrimitiveType
{
    Number,
    Int,
    Float,
    Double,
    Decimal,
    String, // I know, I know that string is not primitive type, I`ll fix it later :/
    Boolean
}

public readonly struct GType(GPrimitiveType kind, bool isArray = false)
{
    public GPrimitiveType Kind { get; } = kind;
    public bool IsArray { get; } = isArray;

    public Type GetClrType() => IsArray
        ? GetBaseType().MakeArrayType()
        : GetBaseType();

    private Type GetBaseType() => Kind switch
    {
        GPrimitiveType.Number => typeof(int),
        GPrimitiveType.Int => typeof(int),
        GPrimitiveType.Float => typeof(float),
        GPrimitiveType.Double => typeof(double),
        GPrimitiveType.Decimal => typeof(decimal),
        GPrimitiveType.String => typeof(string),
        GPrimitiveType.Boolean => typeof(bool),
        _ => throw new NotSupportedException($"Unknown type {Kind}")
    };

    public override string ToString() =>
        IsArray ? $"{Kind}[]" : Kind.ToString();
}