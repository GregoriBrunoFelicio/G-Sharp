using GSharp.Compiler.Lexer.Helpers;
using static GSharp.Compiler.Lexer.Helpers.SymbolTokenMap;

namespace GSharp.Compiler.Lexer;

public class Lexer
{
    private static readonly Dictionary<string, TokenType> KeywordTokenMap = new()
    {
        // Booleans
        ["true"] = TokenType.BooleanTrueLiteral,
        ["false"] = TokenType.BooleanFalseLiteral,

        // Conditionals
        ["if"] = TokenType.If,
        ["else"] = TokenType.Else,

        // Loops
        ["for"] = TokenType.For,
        ["in"] = TokenType.In,
        ["do"] = TokenType.Do,
        ["then"] = TokenType.Then,

        // IO
        ["println"] = TokenType.Println,

        // Logical operators
        ["and"] = TokenType.And,
        ["or"] = TokenType.Or,
        ["not"] = TokenType.Not,

        // Functions
        ["function"] = TokenType.Function,

        // Imports
        ["import"] = TokenType.Import,
        ["as"] = TokenType.As
    };

    private static readonly HashSet<char> NumberSuffixes = ['f', 'F', 'd', 'D', 'm', 'M'];
    private readonly Stack<int> _blockLevelStack = new([0]);

    private readonly List<Token> _tokens = [];
    public readonly string Code;
    private bool _atStartOfLine = true;
    public int Position;

    public Lexer(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new NullReferenceException("Code cannot be null or empty.");
        Code = code;
    }

    public int Line { get; private set; } = 1;
    public int Column { get; private set; } = 1;
    public char Current => !IsAtEnd() ? Code[Position] : '\0';

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

        // blank, whitespace-only, or comment-only line — skip, don't change block level
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

    private bool IsNewLine()
    {
        return Current == '\n' || Current == '\r';
    }

    private bool IsLineComment()
    {
        return Current == '/' && Next() == '/';
    }

    private void SkipLineComment()
    {
        AdvanceWhile(c => c != '\n' && c != '\r');
    }

    private void ConsumeNewLine()
    {
        if (Current == '\r' && Next() == '\n')
            Advance();

        Advance();

        if (!LastTokenIsNewline())
            _tokens.Add(new Token(TokenType.Newline, "\n"));
    }

    private bool LastTokenIsNewline()
    {
        return _tokens.Count > 0 && _tokens[^1].Type == TokenType.Newline;
    }

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

    public Token ReadIdentifier()
    {
        var line = Line;
        var col = Column;
        var start = Position;

        AdvanceWhile(char.IsLetterOrDigit);

        var value = Code[start..Position];
        var tokenType = KeywordTokenMap.GetValueOrDefault(value, TokenType.Identifier);

        return new Token(tokenType, value, line, col);
    }

    public Token ReadNumber()
    {
        var line = Line;
        var col = Column;
        var start = Position;

        AdvanceWhile(char.IsDigit);
        ReadDecimalIfExists();

        var number = Code[start..Position];
        return new Token(TokenType.NumberLiteral, number, line, col);
    }

    private void ReadDecimalIfExists()
    {
        if (IsAtEnd() || Current != '.') return;
        Advance();
        AdvanceWhile(char.IsDigit);

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

        Advance(); // skip opening "

        var start = Position;
        AdvanceWhile(c => c != '"');

        if (IsAtEnd())
            throw new Exception($"{line}: unterminated string literal");

        var word = Code[start..Position];
        Advance(); // skip closing "

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

    public bool IsAtEnd()
    {
        return Position >= Code.Length;
    }
}