namespace G.Sharp.Compiler.AST;

public abstract record Statement;
public record LetStatement(string VariableName, VariableValue VariableValue) : Statement;
public record PrintStatement(string VariableName) : Statement;
