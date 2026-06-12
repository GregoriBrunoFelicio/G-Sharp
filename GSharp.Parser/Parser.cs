using GSharp.AST;
using GSharp.Lexer;
using static GSharp.Parser.Validations;

namespace GSharp.Parser;

public class Parser(List<Token> tokens)
{
    /*Scopes used to detect duplicate declarations.
    Top-level is the initial scope; each function body gets its own scope.
    `if`/`for` do not create scopes, since locals are stored per function.*/
    private readonly Stack<HashSet<string>> _scopes = new([[]]);
    private int _current;

    public void EnterScope() => _scopes.Push([]);
    public void ExitScope() => _scopes.Pop();

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

        while (!IsAtEnd() && !Check(TokenType.EndOfFile))
        {
            if (Match(TokenType.Newline))
                continue;

            expressions.Add(ParseNext());
        }

        return expressions;
    }

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

        if (IsLiteralToken(Current().Type))
            return new ExpressionParser(this).Parse();

        throw new Exception($"{tokens[_current].Line}: unexpected '{tokens[_current].Value}'");
    }

    private bool IsFunctionDeclaration()
    {
        var saved = _current;
        try
        {
            Advance();
            while (Check(TokenType.Identifier))
                Advance();
            
            while (Check(TokenType.Newline))
                Advance();
            return Check(TokenType.Arrow) || Check(TokenType.BlockOpen);
        }
        finally
        {
            _current = saved;
        }
    }

    public Token Consume(TokenType type) =>
        Check(type)
            ? Advance()
            : throw new Exception($"{tokens[_current].Line}: expected '{type}', got '{tokens[_current].Value}'");

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

    public Token Current() => 
        IsAtEnd() ? throw new Exception("unexpected end of input") : tokens[_current];

    private bool IsAtEnd() => _current >= tokens.Count;

    public Token Identifier() => Consume(TokenType.Identifier);

    public void Equals() => Consume(TokenType.Equals);
}