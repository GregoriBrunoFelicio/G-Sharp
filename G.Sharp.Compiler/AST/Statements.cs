namespace G.Sharp.Compiler.AST;

public abstract record Statement;

public record AssignmentStatement(string VariableName, Expression Expression) : Statement;

public record LetStatement(string VariableName, Expression Expression) : Statement; // TODO: Should accept  expression

public record PrintStatement(Expression Expression) : Statement; // TODO: Should accept  expression

public record ForStatement(string Variable, Expression Iterable, List<Statement> Body) : Statement;