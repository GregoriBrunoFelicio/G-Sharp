using G.Sharp.Compiler.Extensions;
using static G.Sharp.Compiler.Lexer.Syntax;

namespace G.Sharp.Compiler.Lexer;

public class Lexer
{
    private readonly string _code;
    private int _position;
    private readonly List<Token> _tokens = [];
    private readonly HashSet<char> _supportedNumberSuffixes = ['f', 'F', 'd', 'D', 'm', 'M'];

    public Lexer(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new NullReferenceException("Code cannot be null or empty.");
        _code = code;
    }

    public List<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            var current = Peek();

            if (current.IsWhitespace())
            {
                Advance();
                continue;
            }

            if (ReadIdentifierOrKeyword()) continue;
            if (ReadNumberLiteral()) continue;
            if (ReadStringLiteral()) continue;
            if (ReadSymbol()) continue;

            throw new Exception($"Unexpected character: '{current}'");
        }

        _tokens.Add(new Token(TokenType.EndOfFile, ""));
        return _tokens;
    }

    private char Peek() => _code[_position];
    private void Advance() => _position++;
    private bool IsAtEnd() => _position >= _code.Length;

    private bool ReadIdentifierOrKeyword()
    {
        if (!Peek().IsLetter()) return false;

        var word = ReadWhile(char.IsLetterOrDigit);

        var token = KeywordTokenMap.TryGetValue(word, out var type)
            ? new Token(type, word)
            : new Token(TokenType.Identifier, word);

        _tokens.Add(token);

        return true;
    }

    private bool ReadNumberLiteral()
    {
        if (!char.IsDigit(Peek())) return false;

        var start = _position;

        ReadWhile(char.IsDigit);

        if (!IsAtEnd() && Peek() == '.')
        {
            Advance();
            ReadWhile(char.IsDigit);
        }

        if (!IsAtEnd() && _supportedNumberSuffixes.Contains(Peek()))
        {
            Advance();
        }

        var number = _code[start.._position];
        _tokens.Add(new Token(TokenType.NumberLiteral, number));
        return true;
    }

    private bool ReadStringLiteral()
    {
        if (!Peek().IsOnlyQuotes()) return false;
        var value = ParseRawStringLiteral();
        _tokens.Add(new Token(TokenType.StringLiteral, value));
        return true;
    }

    private string ParseRawStringLiteral()
    {
        if (Peek() != '"')
            throw new Exception("Expected '\"' at start of string literal");

        Advance();
        var start = _position;

        while (!IsAtEnd() && Peek() != '"')
            _position++;

        if (IsAtEnd())
            throw new Exception("Unterminated string literal. Expected closing '\"'.");

        var value = _code[start.._position];
        Advance();

        return value;
    }

    private bool ReadSymbol()
    {
        var current = Peek();

        if (!SymbolTokenMap.TryGetValue(current, out var tokenType))
            return false;

        _tokens.Add(new Token(tokenType, current.ToString()));
        Advance();
        return true;
    }

    private string ReadWhile(Func<char, bool> condition)
    {
        var start = _position;
        while (!IsAtEnd() && condition(Peek()))
            Advance();
        return _code[start.._position];
    }
}