namespace G.Sharp.Compiler.AST;

public abstract record Statement;

public record AssignmentStatement(string VariableName, VariableValue VariableValue) : Statement;

public record LetStatement(string VariableName, VariableValue VariableValue) : Statement; // TODO: Should accept  expression

public record PrintStatement(string VariableName) : Statement; // TODO: Should accept  expression

public record ForStatement(string Variable, Expression Iterable, List<Statement> Body) : Statement;