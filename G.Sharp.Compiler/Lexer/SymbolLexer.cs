using static G.Sharp.Compiler.Lexer.SymbolTokenMap;

namespace G.Sharp.Compiler.Lexer;

public static class SymbolLexer
{
    public static Token Read(Lexer lexer)
    {
        var current = lexer.Current;

        if (!Symbols.TryGetValue(current, out var type))
            throw new Exception($"Unknown symbol: '{current}'");

        lexer.Advance();

        //TODO: ToString? Maybe use another approach to get the string value
        return new Token(type, current.ToString());
    }
}