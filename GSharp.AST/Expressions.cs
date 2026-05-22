using GSharp.Lexer;

namespace GSharp.AST;

public abstract record Expression;

public record LiteralExpression(object Value) : Expression;

public record BindingExpression(string Name) : Expression;

public record BinaryExpression(Expression Left, TokenType Operator, Expression Right) : Expression;

public record CallExpression(string Callee, List<Expression> Arguments) : Expression;