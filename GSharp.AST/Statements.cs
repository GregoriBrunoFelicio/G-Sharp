namespace GSharp.AST;

public record LetExpression(string BindingName, Expression Value) : Expression;

public record PrintExpression(Expression Value) : Expression;

public record IfExpression(Expression Condition, List<Expression> ThenBody, List<Expression>? ElseBody = null) : Expression;

public record ForExpression(string BindingName, Expression Iterable, List<Expression> Body) : Expression;

public record WhileExpression(Expression Condition, List<Expression> Body) : Expression;
