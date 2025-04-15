namespace G.Sharp.Compiler.AST;

public abstract record VariableValue
{
    public abstract GType Type { get; }
}

public record StringValue(string Value) : VariableValue
{
    public override GType Type => new(GPrimitiveType.String);
}

public record BooleanValue(bool Value) : VariableValue
{
    public override GType Type => new(GPrimitiveType.Boolean);
}

public record ArrayValue(IReadOnlyList<VariableValue> Elements, GType ElementType) : VariableValue
{
    public override GType Type => new(ElementType.Kind, isArray: true);
}

public abstract record NumberValue : VariableValue
{
}

public sealed record IntValue(int Value) : NumberValue
{
    public override GType Type => new(GPrimitiveType.Int);
}

public sealed record FloatValue(float Value) : NumberValue
{
    public override GType Type => new(GPrimitiveType.Float);
}

public sealed record DoubleValue(double Value) : NumberValue
{
    public override GType Type => new(GPrimitiveType.Double);
}

public sealed record DecimalValue(decimal Value) : NumberValue
{
    public override GType Type => new(GPrimitiveType.Decimal);
}