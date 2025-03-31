using G.Sharp.Compiler.Extensions;
using static G.Sharp.Compiler.Lexer.Syntax;

namespace G.Sharp.Compiler.Lexer;

public class Lexer
{
    public readonly string Code;
    public int Position;
    public char Current => !IsAtEnd() ? Code[Position] : '\0';

    private readonly List<Token> _tokens = [];

    public Lexer(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new NullReferenceException("Code cannot be null or empty.");
        Code = code;
    }

    public List<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            if (Current.IsWhitespace())
            {
                Advance();
                continue;
            }

            var token = ReadNextToken();
            _tokens.Add(token);
        }

        _tokens.Add(new Token(TokenType.EndOfFile, ""));
        return _tokens;
    }

    private Token ReadNextToken()
    {
        if (Current.IsLetter())
            return IdentifierLexer.Read(this);

        if (Current.IsNumber())
            return NumberLexer.Read(this);

        if (Current.IsOnlyQuotes())
            return StringLexer.Read(this);

        if (SymbolTokenMap.ContainsKey(Current))
            return SymbolLexer.Read(this);

        throw new Exception($"Unexpected character: '{Current}'");
    }

    public void Advance() => Position++;

    public void AdvanceWhile(Func<char, bool> condition)
    {
        while (!IsAtEnd() && condition(Current))
            Advance();
    }

    public bool IsAtEnd() => Position >= Code.Length;
}