using GSharp.Compiler.Lexer.Helpers;
using static GSharp.Compiler.Lexer.Helpers.Constants;
using static GSharp.Compiler.Lexer.Helpers.SymbolTokenMap;

namespace GSharp.Compiler.Lexer;

public class Lexer
{
    private readonly Stack<int> _blockLevelStack = new([0]);
    private readonly List<Token> _tokens = [];
    private readonly string _code;
    private bool _atStartOfLine = true;
    private int _lineStart;
    public int Position;

    public Lexer(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new NullReferenceException("Code cannot be null or empty.");
        _code = code;
    }

    private int Line { get; set; } = 1;
    private int Column => Position - _lineStart + 1;
    public char Current => !IsAtEnd() ? _code[Position] : '\0';

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
            _tokens.Add(new Token(TokenType.BlockClose, "", Line, Column));
        }

        _tokens.Add(new Token(TokenType.EndOfFile, "", Line, Column));
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
        
        if (IsAtEnd() || IsNewLine() || IsLineComment())
            return;
        var currentLevel = _blockLevelStack.Peek();

        if (spaces > currentLevel)
        {
            _blockLevelStack.Push(spaces);
            _tokens.Add(new Token(TokenType.BlockOpen, "", Line, Column));
        }
        else if (spaces < currentLevel)
        {
            while (_blockLevelStack.Count > 1 && _blockLevelStack.Peek() > spaces)
            {
                _blockLevelStack.Pop();
                _tokens.Add(new Token(TokenType.BlockClose, "", Line, Column));
            }
        }
    }

    private bool IsNewLine() => Current == '\n' || Current == '\r';

    private bool IsLineComment() => Current == '/' && Next() == '/';

    private void SkipLineComment()
    {
        while (!IsAtEnd() && Current != '\n' && Current != '\r')
            Advance();
    }

    private void ConsumeNewLine()
    {
        if (Current == '\r' && Next() == '\n')
            Advance();

        Advance();
        Line++;
        _lineStart = Position;

        if (!LastTokenIsNewline())
            _tokens.Add(new Token(TokenType.Newline, "\n"));
    }

    private bool LastTokenIsNewline() => _tokens.Count > 0 && _tokens[^1].Type == TokenType.Newline;

    private Token ReadNextToken()
    {
        if (Current.IsLetter())
            return ReadIdentifier();

        if (Current.IsNumber())
            return ReadNumber();

        if (Current.IsOnlyQuotes())
            return ReadString();

        if (Symbols.ContainsKey(Current))
            return ReadSymbol();

        throw new Exception($"{Line}: unexpected '{Current}'");
    }

    private Token ReadIdentifier()
    {
        var line = Line;
        var col = Column;
        var start = Position;

        while (!IsAtEnd() && char.IsLetterOrDigit(Current))
            Advance();

        var value = _code[start..Position];
        var tokenType = KeywordTokenMap.GetValueOrDefault(value, TokenType.Identifier);

        return new Token(tokenType, value, line, col);
    }

    public Token ReadNumber()
    {
        var line = Line;
        var col = Column;
        var start = Position;

        while (!IsAtEnd() && char.IsDigit(Current))
            Advance();
        ReadDecimalIfExists();

        var number = _code[start..Position];
        return new Token(TokenType.NumberLiteral, number, line, col);
    }

    private void ReadDecimalIfExists()
    {
        if (IsAtEnd() || Current != '.') return;
        Advance();
        while (!IsAtEnd() && char.IsDigit(Current))
            Advance();

        ReadNumberSuffix();
    }

    private void ReadNumberSuffix()
    {
        if (IsAtEnd() || !NumberSuffixes.Contains(Current)) return;
        Advance();
    }

    public Token ReadString()
    {
        var line = Line;
        var col = Column;

        Advance(); 

        var start = Position;
        while (!IsAtEnd() && Current != '"')
            Advance();

        if (IsAtEnd())
            throw new Exception($"{line}: unterminated string literal");

        var word = _code[start..Position];
        Advance(); 

        return new Token(TokenType.StringLiteral, word, line, col);
    }

    public Token ReadSymbol()
    {
        var line = Line;
        var col = Column;
        var current = Current;
        var next = Next();

        switch (current)
        {
            case '-' when next == '>':
                Advance();
                Advance();
                return new Token(TokenType.ThinArrow, "->", line, col);
            case '=' when next == '>':
                Advance();
                Advance();
                return new Token(TokenType.Arrow, "=>", line, col);
            case '>' when next == '=':
                Advance();
                Advance();
                return new Token(TokenType.GreaterThanOrEqual, ">=", line, col);
            case '<' when next == '=':
                Advance();
                Advance();
                return new Token(TokenType.LessThanOrEqual, "<=", line, col);
            case '=' when next == '=':
                Advance();
                Advance();
                return new Token(TokenType.EqualEqual, "==", line, col);
            case '!' when next == '=':
                Advance();
                Advance();
                return new Token(TokenType.NotEqual, "!=", line, col);
        }

        if (Symbols.TryGetValue(current, out var tokenType))
        {
            Advance();
            return new Token(tokenType, current.ToString(), line, col);
        }

        throw new Exception($"{line}: unexpected '{current}'");
    }

    public void Advance() => Position++;

    public char Next()
    {
        var next = Position + 1;
        return next < _code.Length ? _code[next] : '\0';
    }

    public void AdvanceWhile(Func<char, bool> condition)
    {
        while (!IsAtEnd() && condition(Current))
            Advance();
    }

    public bool IsAtEnd() => Position >= _code.Length;
}