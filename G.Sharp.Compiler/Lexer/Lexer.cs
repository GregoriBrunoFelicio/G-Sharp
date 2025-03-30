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

            if (token is not null)
            {
                _tokens.Add(token);
                continue;
            }

            throw new Exception($"Unexpected character: '{Current}'");
        }

        _tokens.Add(new Token(TokenType.EndOfFile, ""));
        return _tokens;
    }

    public void Advance() => Position++;
    
    public bool IsAtEnd() => Position >= Code.Length;

    public void AdvanceWhile(Func<char, bool> condition)
    {
        while (!IsAtEnd() && condition(Current))
            Advance();
    }

    private Token? ReadNextToken()
    {
        if (char.IsLetter(Current))
            return IdentifierLexer.Read(this);

        if (char.IsDigit(Current))
            return NumberLexer.Read(this);

        if (Current == '"')
            return StringLexer.Read(this);

        if (SymbolTokenMap.ContainsKey(Current))
            return SymbolLexer.Read(this);

        return null;
    }
}