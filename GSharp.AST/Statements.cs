namespace GSharp.AST;

public abstract record Statement;

public record AssignmentStatement(string VariableName, Expression Expression) : Statement;

public record LetStatement(string VariableName, Expression Expression) : Statement; 

public record PrintStatement(Expression Expression) : Statement; 

public record ForStatement(string Variable, Expression Iterable, List<Statement> Body) : Statement;

public record IfStatement(Expression Condition, List<Statement> ThenBody, List<Statement>? ElseBody = null) : Statement;