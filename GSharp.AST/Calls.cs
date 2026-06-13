namespace GSharp.AST;

public record CallExpression(string Callee, List<Expression> Arguments) : Expression;

public record ModuleCallExpression(string Module, string Function, List<Expression> Arguments) : Expression;
