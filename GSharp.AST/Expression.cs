using GSharp.Lexer;

namespace GSharp.AST;

/// <summary>
/// Base type for every AST node.
///
/// <see cref="Line"/> and <see cref="Column"/> are 1-based source positions (matching the
/// lexer's <see cref="Token"/> coordinates) marking where the construct begins. They default
/// to 0 ("no position"), so nodes built without span information keep working unchanged.
/// The parser fills them in for the leaf-ish nodes the LSP needs for hover: literals,
/// bindings, calls, let bindings and function declarations.
/// </summary>
public abstract record Expression
{
    public int Line { get; init; }
    public int Column { get; init; }
}

public record LiteralExpression(object Value) : Expression;

public record BindingExpression(string Name) : Expression;

public record BinaryExpression(Expression Left, TokenType Operator, Expression Right) : Expression;
