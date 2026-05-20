using GSharp.Lexer.Helpers;
using static GSharp.Lexer.Helpers.SymbolTokenMap;

namespace GSharp.Lexer;

public class Lexer
{
    public readonly string Code;
    public int Position;
    public char Current => !IsAtEnd() ? Code[Position] : '\0';

    private readonly List<Token> _tokens = [];
    private bool _atStartOfLine = true;
    private readonly Stack<int> _indentStack = new([0]);

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
                HandleIndentationChange();
                continue;
            }

            if (Current.IsWhitespace())
            {
                Advance();
                continue;
            }

            var token = ReadNextToken();
            _tokens.Add(token);
        }

        while (_indentStack.Count > 1)
        {
            _indentStack.Pop();
            _tokens.Add(new Token(TokenType.Dedent, ""));
        }

        _tokens.Add(new Token(TokenType.EndOfFile, ""));
        return _tokens;
    }

    private void HandleIndentationChange()
    {
        var indent = 0;
        while (!IsAtEnd() && Current == ' ')
        {
            indent++;
            Advance();
        }

        // blank or whitespace-only line — skip, don't change indent level
        if (IsAtEnd() || IsNewLine())
            return;

        var current = _indentStack.Peek();

        if (indent > current)
        {
            _indentStack.Push(indent);
            _tokens.Add(new Token(TokenType.Indent, ""));
        }
        else if (indent < current)
        {
            while (_indentStack.Count > 1 && _indentStack.Peek() > indent)
            {
                _indentStack.Pop();
                _tokens.Add(new Token(TokenType.Dedent, ""));
            }
        }
    }

    private bool IsNewLine() => Current == '\n' || Current == '\r';

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

        throw new Exception($"Unexpected character: '{Current}'");
    }

    public void Advance() => Position++;
    
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
