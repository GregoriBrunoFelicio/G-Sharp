using GSharp.Lexer;

namespace GSharp.AST;

public abstract record Expression;

public record LiteralExpression(object Value) : Expression;

public record BindingExpression(string Name) : Expression;

public record BinaryExpression(Expression Left, TokenType Operator, Expression Right) : Expression;

public record CallExpression(string Callee, List<Expression> Arguments) : Expression;

public record LetExpression(string BindingName, Expression Value) : Expression;

public record PrintExpression(Expression Value) : Expression;

public record IfExpression(Expression Condition, List<Expression> ThenBody, List<Expression>? ElseBody = null) : Expression;

public record ForExpression(string BindingName, Expression Iterable, List<Expression> Body) : Expression;

public record WhileExpression(Expression Condition, List<Expression> Body) : Expression;

public record FunctionDeclaration(string Name, List<string> Parameters, List<Expression> Body) : Expression;

public record ImportDeclaration(string ModuleName) : Expression;

public record QualifiedCallExpression(string Module, string Function, List<Expression> Arguments) : Expression;
