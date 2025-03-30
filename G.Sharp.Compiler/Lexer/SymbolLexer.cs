using static G.Sharp.Compiler.Lexer.Syntax;

namespace G.Sharp.Compiler.Lexer;

public static class SymbolLexer
{
    public static Token Read(Lexer lexer)
    {
        var current = lexer.Current;

        if (!SymbolTokenMap.TryGetValue(current, out var type))
            throw new Exception($"Unknown symbol: '{current}'");

        lexer.Advance();
        return new Token(type, current.ToString());
    }
}