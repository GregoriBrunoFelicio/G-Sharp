namespace GSharp.AST;

public abstract record VariableValue
{
    public abstract GType Type { get; }

    public override string ToString()
    {
        // TODO: think a better approach to do this..
        var valueProp = GetType().GetProperty("Value");
        var val = valueProp?.GetValue(this);
        return val?.ToString() ?? "<uninitialized>";
    }
}

public sealed record StringValue(string Value) : VariableValue
{
    public override GType Type => new GStringType();
}

public sealed record BooleanValue(bool Value) : VariableValue
{
    public override GType Type => new GBooleanType();
}

public abstract record NumberValue : VariableValue;

public sealed record IntValue(int Value) : NumberValue
{
    public override GType Type => new GNumberType();
}

public sealed record FloatValue(float Value) : NumberValue
{
    public override GType Type => new GNumberType();
}

public sealed record DoubleValue(double Value) : NumberValue
{
    public override GType Type => new GNumberType();
}

public sealed record DecimalValue(decimal Value) : NumberValue
{
    public override GType Type => new GNumberType();
}

public sealed record ArrayValue(IReadOnlyList<VariableValue> Elements, GType ElementType) : VariableValue
{
    public override GType Type => new GArrayType(ElementType);
}
