using System.Linq.Expressions;

namespace G.Sharp.Compiler.AST;

public abstract record Statement;

public record AssignmentStatement(string VariableName, VariableValue VariableValue) : Statement;
public record LetStatement(string VariableName, VariableValue VariableValue) : Statement;

public record PrintStatement(string VariableName) : Statement;
public record IfStatement(
    Expression Condition,
    List<Statement> ThenBranch,
    List<Statement>? ElseBranch
) : Statement;