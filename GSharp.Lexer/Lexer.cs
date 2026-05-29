using GSharp.Lexer.Helpers;
using static GSharp.Lexer.Helpers.SymbolTokenMap;

namespace GSharp.Lexer;

public class Lexer
{
    public readonly string Code;
    public int Position;
    public int Line { get; private set; } = 1;
    public int Column { get; private set; } = 1;
    public char Current => !IsAtEnd() ? Code[Position] : '\0';

    private readonly List<Token> _tokens = [];
    private bool _atStartOfLine = true;
    private readonly Stack<int> _blockLevelStack = new([0]);

    public Lexer(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new NullReferenceException("Code cannot be null or empty.");
        Code = code;
    }

    public List<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            if (IsNewLine())
            {
                ConsumeNewLine();
                _atStartOfLine = true;
                continue;
            }

            if (_atStartOfLine)
            {
                _atStartOfLine = false;
                HandleBlockLevelChange();
                continue;
            }

            if (Current.IsWhitespace())
            {
                Advance();
                continue;
            }

            if (IsLineComment())
            {
                SkipLineComment();
                continue;
            }

            var token = ReadNextToken();
            _tokens.Add(token);
        }

        while (_blockLevelStack.Count > 1)
        {
            _blockLevelStack.Pop();
            _tokens.Add(new Token(TokenType.BlockClose, ""));
        }

        _tokens.Add(new Token(TokenType.EndOfFile, ""));
        return _tokens;
    }

    private void HandleBlockLevelChange()
    {
        var spaces = 0;
        while (!IsAtEnd() && Current == ' ')
        {
            spaces++;
            Advance();
        }

        // blank or whitespace-only line — skip, don't change block level
        if (IsAtEnd() || IsNewLine())
            return;

        var currentLevel = _blockLevelStack.Peek();

        if (spaces > currentLevel)
        {
            _blockLevelStack.Push(spaces);
            _tokens.Add(new Token(TokenType.BlockOpen, ""));
        }
        else if (spaces < currentLevel)
        {
            while (_blockLevelStack.Count > 1 && _blockLevelStack.Peek() > spaces)
            {
                _blockLevelStack.Pop();
                _tokens.Add(new Token(TokenType.BlockClose, ""));
            }
        }
    }

    private bool IsNewLine() => Current == '\n' || Current == '\r';

    private bool IsLineComment() => Current == '/' && Next() == '/';

    private void SkipLineComment() => AdvanceWhile(c => c != '\n' && c != '\r');

    private void ConsumeNewLine()
    {
        if (Current == '\r' && Next() == '\n')
            Advance();

        Advance();

        if (!LastTokenIsNewline())
            _tokens.Add(new Token(TokenType.Newline, "\n"));
    }

    private bool LastTokenIsNewline() =>
        _tokens.Count > 0 && _tokens[^1].Type == TokenType.Newline;

    private Token ReadNextToken()
    {
        if (Current.IsLetter())
            return IdentifierLexer.Read(this);

        if (Current.IsNumber())
            return NumberLexer.Read(this);

        if (Current.IsOnlyQuotes())
            return StringLexer.Read(this);

        if (Symbols.ContainsKey(Current))
            return SymbolLexer.Read(this);

        throw new Exception($"{Line}: unexpected '{Current}'");
    }

    public void Advance()
    {
        if (!IsAtEnd() && Code[Position] == '\n')
        {
            Line++;
            Column = 1;
        }
        else
        {
            Column++;
        }
        Position++;
    }

    public char Next()
    {
        var next = Position + 1;
        return next < Code.Length ? Code[next] : '\0';
    }

    public void AdvanceWhile(Func<char, bool> condition)
    {
        while (!IsAtEnd() && condition(Current))
            Advance();
    }

    public bool IsAtEnd() => Position >= Code.Length;
}
