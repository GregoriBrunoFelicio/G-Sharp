namespace G.Sharp.Compiler.AST;

public abstract record VariableValue;
public record BooleanValue(bool Value) : VariableValue;
public record NumberValue(int Value) : VariableValue;
public record StringValue(string Value) : VariableValue;
