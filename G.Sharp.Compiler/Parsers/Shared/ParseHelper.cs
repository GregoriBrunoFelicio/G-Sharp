using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Parsers.Shared;

public class ParserHelper(IEnumerable<Token> tokens)
{
    private readonly List<Token> _tokens = tokens.ToList();
    public int Current;

    /// <summary>
    /// Token navigation methods
    /// Core utilities for moving through the token stream, matching and consuming tokens,
    /// and checking whether the parser has reached the end of the input.
    /// </summary>
    public Token Consume(TokenType type)
    {
        if (Check(type))
            return Advance();
        throw new Exception($"Expected token {type}, got {_tokens[Current].Type}");
    }

    public bool Match(TokenType type)
    {
        if (!Check(type)) return false;
        Advance();
        return true;
    }

    public bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return _tokens[Current].Type == type;
    }

    public Token Advance()
    {
        if (!IsAtEnd()) Current++;
        return _tokens[Current - 1];
    }

    public bool IsAtEnd() => Current >= _tokens.Count;
}