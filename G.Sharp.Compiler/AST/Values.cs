namespace G.Sharp.Compiler.AST;

public abstract record VariableValue
{
    public abstract Type Type { get; }
}

public record StringValue(string Value) : VariableValue
{
    public override Type Type => Type.String;
}

public record BooleanValue(bool Value) : VariableValue
{
    public override Type Type => Type.Boolean;
}

public abstract record NumberValue : VariableValue
{
    public override Type Type => Type.Number;
}

public record IntValue(int Value) : NumberValue;

public record FloatValue(float Value) : NumberValue;

public record DoubleValue(double Value) : NumberValue;

public record DecimalValue(decimal Value) : NumberValue;



