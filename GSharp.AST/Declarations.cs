namespace GSharp.AST;

public record FunctionDeclaration(string Name, List<string> Parameters, List<Expression> Body) : Expression;

public record ImportDeclaration(string ModuleName) : Expression;
