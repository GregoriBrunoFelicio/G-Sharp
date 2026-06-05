using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class Parser(List<Token> tokens)
{
    private int _current;

    // Lexical scopes for the "already declared" check. The bottom of the stack is the
    // top-level scope; FunctionParser pushes a new scope for each function body. Granularity
    // is per-function — `if`/`for` do NOT open scopes (the codegen stores locals flat per
    // function), so a name declared in one function never collides with the same name at the
    // top level or in another function.
    private readonly Stack<HashSet<string>> _scopes = new([[]]);

    public void EnterScope() => _scopes.Push([]);

    public void ExitScope() => _scopes.Pop();

    public bool IsDeclaredInCurrentScope(string name)
    {
        var currentScope = _scopes.Peek();
        return currentScope.Contains(name);
    }

    // Declares a name in the current scope, rejecting a duplicate in that same scope
    // (let is immutable — no reassignment, no same-scope shadowing).
    public void DeclareBinding(string name)
    {
        var currentScope = _scopes.Peek();
        var alreadyDeclared = currentScope.Contains(name);

        if (alreadyDeclared)
            throw new Exception($"Binding '{name}' already declared.");

        currentScope.Add(name);
    }

    public List<Expression> Parse()
    {
        var expressions = new List<Expression>();

        while (IsNotEndOfFile())
        {
            if (Match(TokenType.Newline))
                continue;

            expressions.Add(ParseNext());
        }

        return expressions;
    }

    private bool IsNotEndOfFile() =>
        _current < tokens.Count
        && tokens[_current].Type != TokenType.EndOfFile;

    public Expression ParseNext()
    {
        if (Check(TokenType.Import))
            return new ImportParser(this).Parse();

        if (Check(TokenType.Let))
            return new LetParser(this).Parse();

        if (Check(TokenType.Println))
            return new PrintParser(this).Parse();

        if (Check(TokenType.For))
            return new ForParser(this).Parse();

        if (Check(TokenType.If))
            return new IfParser(this).Parse();

        if (Check(TokenType.Identifier))
        {
            if (IsFunctionDeclaration())
                return new FunctionParser(this).Parse();

            return new ExpressionParser(this).Parse();
        }

        if (Check(TokenType.NumberLiteral) || Check(TokenType.StringLiteral) ||
            Check(TokenType.BooleanTrueLiteral) || Check(TokenType.BooleanFalseLiteral))
        {
            return new ExpressionParser(this).Parse();
        }

        throw new Exception($"{tokens[_current].Line}: unexpected '{tokens[_current].Value}'");
    }

    private bool IsFunctionDeclaration()
    {
        var saved = _current;
        try
        {
            Advance(); // skip name
            while (Check(TokenType.Identifier))
                Advance(); // skip params
            while (Match(TokenType.Newline)) { }
            return Check(TokenType.Arrow) || Check(TokenType.BlockOpen);
        }
        finally
        {
            _current = saved;
        }
    }

    public Token Consume(TokenType type) =>
        Check(type) ?
            Advance() :
            throw new Exception($"{tokens[_current].Line}: expected '{type}', got '{tokens[_current].Value}'");

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
        if (IsAtEnd()) throw new Exception("unexpected end of input");
        return tokens[_current];
    }

    private bool IsAtEnd() => _current >= tokens.Count;
    public Token Identifier() => Consume(TokenType.Identifier);
    public void Equals() => Consume(TokenType.Equals);
}
