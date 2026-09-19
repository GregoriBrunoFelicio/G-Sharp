namespace GSharp.Compiler.AST;

public record CallExpression(string Callee, List<Expression> Arguments) : Expression;

// `name.fn args`. Parsed as a module call, but when `name` turns out to be a variable rather than a module
// the type checker/codegen treat it as a member call on `Receiver` (set by the parser to an identifier
// for `name`, so the receiver's type is recorded in the type map like any other expression).
public record ModuleCallExpression(string Module, string Function, List<Expression> Arguments) : Expression
{
    public Expression? Receiver { get; init; }
}

// `receiver.member args` where the receiver is any expression (e.g. `time.now.year`, `(d).month`).
// Sugar for `<module of receiver's type>.member receiver args`, resolved from the receiver's type.
public record MemberCallExpression(Expression Receiver, string Member, List<Expression> Arguments) : Expression;