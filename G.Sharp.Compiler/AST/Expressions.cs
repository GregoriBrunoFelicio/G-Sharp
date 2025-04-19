using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.AST;

public abstract record Expression;

public record LiteralExpression(VariableValue Value) : Expression;

public record VariableExpression(string Name) : Expression;

public record BinaryExpression(Expression Left, TokenType Operator, Expression Right) : Expression;