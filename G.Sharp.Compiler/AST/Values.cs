namespace G.Sharp.Compiler.AST;

public abstract record VariableValue
{
    public abstract Type Type { get; }
}

public record NumberValue(int Value) : VariableValue
{
    public override Type Type => Type.Number;
}

public record StringValue(string Value) : VariableValue
{
    public override Type Type => Type.String;
}

public record BooleanValue(bool Value) : VariableValue
{
    public override Type Type => Type.Boolean;
}

