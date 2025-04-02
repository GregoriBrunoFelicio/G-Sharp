namespace G.Sharp.Compiler.AST;

public enum GPrimitiveType
{
    Number,
    String, // I know, I know that string is not primitive type, I`ll fix it later :/
    Boolean,
}

public readonly struct GType(GPrimitiveType kind, bool isArray = false)
{
    public GPrimitiveType Kind { get; } = kind;
    public bool IsArray { get; } = isArray;
}
