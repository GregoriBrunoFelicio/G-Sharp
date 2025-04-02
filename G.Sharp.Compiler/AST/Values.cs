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

public abstract record NumberValue : VariableValue
{
    public override GType Type => new(GPrimitiveType.Number);
}

public record IntValue(int Value) : NumberValue;

public record FloatValue(float Value) : NumberValue;

public record DoubleValue(double Value) : NumberValue;

public record DecimalValue(decimal Value) : NumberValue;

public record ArrayValue(IReadOnlyList<VariableValue> Elements, GType ElementType) : VariableValue
{
    public override GType Type => new(ElementType.Kind, isArray: true);
}