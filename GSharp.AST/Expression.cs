using GSharp.Lexer;

namespace GSharp.AST;

public abstract record Expression
{
    public int Line { get; init; }
    public int Column { get; init; }
}

public record LiteralExpression(object Value) : Expression;

public record IdentifierExpression(string Name) : Expression;

public record BinaryExpression(Expression Left, TokenType Operator, Expression Right) : Expression;

public record BindingExpression(string BindingName, Expression Value) : Expression;

public record PrintExpression(Expression Value) : Expression;

public record IfExpression(Expression Condition, List<Expression> ThenBody, List<Expression>? ElseBody = null) : Expression;

public record ForExpression(string BindingName, Expression Iterable, List<Expression> Body) : Expression;

