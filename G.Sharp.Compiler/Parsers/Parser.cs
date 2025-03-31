using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Parsers;

public class Parser(List<Token> tokens)
{
    private int _current;
    public readonly Dictionary<string, GType> VariablesDeclared = [];

    public List<Statement> Parse()
    {
        var statements = new List<Statement>();

        while (IsNotEndOfFile())
        {
            var statement = ParseNextStatement();
            statements.Add(statement);
        }

        return statements;
    }

    private bool IsNotEndOfFile() =>
        _current < tokens.Count
        && tokens[_current].Type
        != TokenType.EndOfFile;

    private Statement ParseNextStatement()
    {
        if (Match(TokenType.Let))
            return new LetParser(this).Parse();

        if (Match(TokenType.Println))
            return new PrintParser(this).Parse();
        
        if (Check(TokenType.Identifier))
            return new AssignmentParser(this).Parse();

        throw new Exception("Invalid statement");
    }

    /// <summary>
    /// Token navigation methods
    /// Core utilities for moving through the token stream, matching and consuming tokens,
    /// and checking whether the parser has reached the end of the input.
    /// </summary>
    public Token Consume(TokenType type)
    {
        if (Check(type))
            return Advance();
        throw new Exception($"Expected token {type}, got {tokens[_current].Type}");
    }

    public bool Match(TokenType type)
    {
        if (!Check(type)) return false;
        Advance();
        return true;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return tokens[_current].Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return tokens[_current - 1];
    }

    private bool IsAtEnd() => _current >= tokens.Count;
}