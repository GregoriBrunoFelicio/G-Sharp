namespace GSharp.AST;

public record FunctionDeclaration(string Name, List<string> Parameters, List<Expression> Body) : Expression;

public record ImportDeclaration(string ModuleName) : Expression;

// Import of a .NET type for interop, e.g. `import system.math`.
// The dot in the import distinguishes it from a G# module import (`import math`).
//   TypeName — the dotted name as written ("system.math"); resolved case-insensitively to a CLR type.
//   Alias    — the lowercased last segment ("math"); the name used at call sites (math.sqrt 16.0d).
public record DotnetImportDeclaration(string TypeName, string Alias) : Expression;
