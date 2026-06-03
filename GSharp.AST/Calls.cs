namespace GSharp.AST;

public record CallExpression(string Callee, List<Expression> Arguments) : Expression;

public record QualifiedCallExpression(string Module, string Function, List<Expression> Arguments) : Expression;
