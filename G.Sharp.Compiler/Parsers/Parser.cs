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

    public Statement ParseNextStatement()
    {
        if (Match(TokenType.Let))
            return new LetParser(this).Parse();

        if (Match(TokenType.Println))
            return new PrintParser(this).Parse();

        if (Match(TokenType.For))
            return new ForParser(this).Parse();

        if (Check(TokenType.Identifier))
            return new AssignmentParser(this).Parse();

        throw new Exception("Invalid statement");
    }

    public Token Consume(TokenType type)
    {
        if (Check(type))
            return Advance();
        throw new Exception($"Expected token {type}, got {tokens[_current].Type}");
    }

    private Token Advance()
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
        if (_current == 0) throw new Exception("No previous token");
        return tokens[_current - 1];
    }

    public Token Current()
    {
        if (IsAtEnd()) throw new Exception("Unexpected end of input.");
        return tokens[_current];
    }

    private bool IsAtEnd() => _current >= tokens.Count;

    public Token Identifier() => Consume(TokenType.Identifier);

    public void Colon() => Consume(TokenType.Colon);

    public void Equals() => Consume(TokenType.Equals);

    public void Semicolon() => Consume(TokenType.Semicolon);
}