namespace G.Sharp.Compiler.AST;

public abstract record Expression;

public record LiteralExpression(VariableValue Value) : Expression;

public record VariableExpression(string Name) : Expression;
