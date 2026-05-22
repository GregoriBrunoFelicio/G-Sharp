namespace GSharp.AST;

public abstract record Statement;

public record LetStatement(string BindingName, Expression Expression) : Statement;

public record PrintStatement(Expression Expression) : Statement;

public record ForStatement(string BindingName, Expression Iterable, List<Statement> Body) : Statement;
public record WhileStatement(Expression Condition, List<Statement> Body) : Statement;

public record IfStatement(Expression Condition, List<Statement> ThenBody, List<Statement>? ElseBody = null) : Statement;

public record FunctionDeclaration(string Name, List<string> Parameters, List<Statement> Body) : Statement;

public record ExpressionStatement(Expression Expression) : Statement;