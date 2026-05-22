using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class Parser(List<Token> tokens)
{
    private int _current;
    public readonly HashSet<string> DeclaredBindings = [];

    public List<Statement> Parse()
    {
        var statements = new List<Statement>();

        while (IsNotEndOfFile())
        {
            if (Match(TokenType.Newline))
                continue;

            var statement = ParseNextStatement();
            statements.Add(statement);
        }

        return statements;
    }

    private bool IsNotEndOfFile() =>
        _current < tokens.Count
        && tokens[_current].Type != TokenType.EndOfFile;

    public Statement ParseNextStatement()
    {
        if (Check(TokenType.Let))
            return new LetParser(this).Parse();

        if (Check(TokenType.Println))
            return new PrintParser(this).Parse();

        if (Check(TokenType.For))
            return new ForParser(this).Parse();

        if (Check(TokenType.While))
            return new WhileParser(this).Parse();

        if (Check(TokenType.If))
            return new IfParser(this).Parse();

        if (Check(TokenType.Identifier))
        {
            if (Peek() == TokenType.LeftParen && IsFunctionDeclaration())
                return new FunctionParser(this).Parse();

            return new ExpressionStatement(new ExpressionParser(this).Parse());
        }

        if (Check(TokenType.NumberLiteral) || Check(TokenType.StringLiteral) ||
            Check(TokenType.BooleanTrueLiteral) || Check(TokenType.BooleanFalseLiteral))
        {
            return new ExpressionStatement(new ExpressionParser(this).Parse());
        }

        throw new Exception($"Invalid statement: {tokens[_current].Type}");
    }

    private bool IsFunctionDeclaration()
    {
        var saved = _current;
        try
        {
            Advance(); // name
            Advance(); // (
            while (!Check(TokenType.RightParen) && !IsAtEnd())
                Advance();
            if (!Match(TokenType.RightParen)) return false;
            while (Match(TokenType.Newline)) { } // skip over any newlines oO
            return Check(TokenType.Arrow) || Check(TokenType.BlockOpen);
        }
        finally
        {
            _current = saved;
        }
    }

    private TokenType Peek(int ahead = 1)
    {
        var idx = _current + ahead;
        return idx < tokens.Count ? tokens[idx].Type : TokenType.EndOfFile;
    }

    public Token Consume(TokenType type) =>
        Check(type) ?
            Advance() :
            throw new Exception($"Expected token {type}, got {tokens[_current].Type}");

    public Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return tokens[_current - 1];
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
        return tokens[_current].Type == type;
    }

    public Token Previous()
    {
        if (_current == 0) throw new Exception("No previous token.");
        return tokens[_current - 1];
    }

    public Token Current()
    {
        if (IsAtEnd()) throw new Exception("Unexpected end of input.");
        return tokens[_current];
    }

    private bool IsAtEnd() => _current >= tokens.Count;
    public Token Identifier() => Consume(TokenType.Identifier);
    public void Equals() => Consume(TokenType.Equals);
}
