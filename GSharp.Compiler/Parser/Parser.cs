using GSharp.Compiler.AST;
using GSharp.Compiler.Lexer;
using static GSharp.Compiler.Parser.Validations;

namespace GSharp.Compiler.Parser;

public class Parser(List<Token> tokens)
{
    private static readonly HashSet<TokenType> AtomStartTokens =
    [
        TokenType.Identifier,
        TokenType.LeftParen,
        TokenType.LeftBracket
    ];

    /*Scopes used to detect duplicate declarations.
    Top-level is the initial scope; each function body gets its own scope.
    `if`/`for` do not create scopes, since locals are stored per function.*/
    private readonly Stack<HashSet<string>> _scopes = new([[]]);
    private int _current;

    private void EnterScope()
    {
        _scopes.Push([]);
    }

    private void ExitScope()
    {
        _scopes.Pop();
    }

    private void DeclareBinding(string name)
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

            var expression = ParseNext();

            expressions.Add(expression);
        }

        return expressions;
    }

    private Expression ParseNext()
    {
        if (Check(TokenType.Import))
            return ParseImport();

        if (Check(TokenType.Println))
            return ParsePrint();

        if (Check(TokenType.For))
            return ParseFor();

        if (Check(TokenType.If))
            return ParseIf();

        if (Check(TokenType.Identifier))
        {
            if (IsLetBinding())
                return ParseBinding();

            if (IsFunctionDeclaration())
                return ParseFunction();

            return ParseExpression();
        }

        if (IsLiteralToken(Current().Type))
            return ParseExpression();

        throw new Exception($"{tokens[_current].Line}: unexpected '{tokens[_current].Value}'");
    }

    private bool IsLetBinding()
    {
        var saved = _current;
        try
        {
            Advance();
            while (Check(TokenType.Newline))
                Advance();
            return Check(TokenType.ThinArrow);
        }
        finally
        {
            _current = saved;
        }
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

    private Token Consume(TokenType type)
    {
        return Check(type)
            ? Advance()
            : throw new Exception($"{tokens[_current].Line}: expected '{type}', got '{tokens[_current].Value}'");
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return tokens[_current - 1];
    }

    private bool Match(TokenType type)
    {
        if (!Check(type)) return false;
        Advance();
        return true;
    }

    private bool Check(TokenType type)
    {
        return !IsAtEnd() && tokens[_current].Type == type;
    }

    private Token Current()
    {
        return IsAtEnd() ? throw new Exception("unexpected end of input") : tokens[_current];
    }

    private bool IsAtEnd()
    {
        return _current >= tokens.Count;
    }

    private Token Identifier()
    {
        return Consume(TokenType.Identifier);
    }

    private Expression ParseImport()
    {
        Consume(TokenType.Import);
        var name = Consume(TokenType.Identifier).Value;
        return new ImportDeclaration(name);
    }

    private PrintExpression ParsePrint()
    {
        Consume(TokenType.Println);
        var value = ParseExpression();
        return new PrintExpression(value);
    }

    private ForExpression ParseFor()
    {
        Consume(TokenType.For);

        var bindingName = Identifier().Value;

        Consume(TokenType.In);

        var iterable = ParseExpression();

        Consume(TokenType.Do);
        Match(TokenType.Newline);
        Consume(TokenType.BlockOpen);

        var body = new List<Expression>();

        while (!Check(TokenType.BlockClose))
        {
            if (Match(TokenType.Newline))
                continue;

            body.Add(ParseNext());
        }

        Consume(TokenType.BlockClose);

        return new ForExpression(bindingName, iterable, body);
    }

    private IfExpression ParseIf()
    {
        Consume(TokenType.If);

        var condition = ParseExpression();

        Consume(TokenType.Then);

        var thenBody = Check(TokenType.Newline) ? ParseIfBlockBody() : ParseIfInlineBody();

        List<Expression>? elseBody = null;

        if (Match(TokenType.Else))
            elseBody = Check(TokenType.Newline) ? ParseIfBlockBody() : ParseIfInlineBody();

        return new IfExpression(condition, thenBody, elseBody);
    }

    private List<Expression> ParseIfBlockBody()
    {
        Match(TokenType.Newline);
        Consume(TokenType.BlockOpen);

        var expressions = new List<Expression>();

        while (!Check(TokenType.BlockClose))
        {
            if (Match(TokenType.Newline))
                continue;

            expressions.Add(ParseNext());
        }

        Consume(TokenType.BlockClose);

        return expressions;
    }

    private List<Expression> ParseIfInlineBody()
    {
        return [ParseNext()];
    }

    private BindingExpression ParseBinding()
    {
        var (_, bindingName, line, column) = GetBindingNameToken();

        Consume(TokenType.ThinArrow);
        var value = ParseExpression();

        DeclareBinding(bindingName);

        return new BindingExpression(bindingName, value) { Line = line, Column = column };
    }

    private Token GetBindingNameToken()
    {
        var token = Identifier();
        ValidateBindingName(token);
        return token;
    }

    private static void ValidateBindingName(Token token)
    {
        if (!IsValidBindingName(token.Value))
            throw new Exception($"{token.Line}: invalid binding name '{token.Value}'");

        if (IsReserved(token.Value))
            throw new Exception($"'{token.Value}' is a reserved keyword.");
    }

    private FunctionDeclaration ParseFunction()
    {
        var nameToken = Identifier();
        var name = nameToken.Value;

        var parameters = new List<string>();
        while (Check(TokenType.Identifier))
            parameters.Add(Identifier().Value);

        // The function name lives in the enclosing scope; its body gets a fresh scope.
        DeclareBinding(name);

        var body = ParseScopedFunctionBody(parameters);

        // Span points at the function name so hovering it reports the full signature.
        return new FunctionDeclaration(name, parameters, body) { Line = nameToken.Line, Column = nameToken.Column };
    }

    // Parses the body inside a new lexical scope seeded with the parameter names, so a binding
    // can't reuse a parameter name (the codegen resolves parameters before locals, which would
    // otherwise make such a binding silently unreadable). The scope is always popped, even if the
    // body throws.
    private List<Expression> ParseScopedFunctionBody(List<string> parameters)
    {
        EnterScope();
        try
        {
            foreach (var parameter in parameters)
                DeclareBinding(parameter);

            if (Match(TokenType.Arrow))
                return [ParseNext()];

            Match(TokenType.Newline);
            Consume(TokenType.BlockOpen);
            var body = ParseFunctionBody();
            Consume(TokenType.BlockClose);
            return body;
        }
        finally
        {
            ExitScope();
        }
    }

    private List<Expression> ParseFunctionBody()
    {
        var expressions = new List<Expression>();
        while (!Check(TokenType.BlockClose))
        {
            if (Match(TokenType.Newline)) continue;
            expressions.Add(ParseNext());
        }

        return expressions;
    }

    private LiteralExpression ParseArray()
    {
        var elements = new List<object>();
        Type? elementType = null;

        while (!Check(TokenType.RightBracket))
        {
            var expr = ParseExpression();

            if (expr is not LiteralExpression lit)
                throw new Exception("Only literal expressions are supported in arrays for now.");

            var value = lit.Value;
            elementType ??= value?.GetType();

            if (value?.GetType() != elementType)
                throw new Exception("Array literals must contain elements of the same type.");

            elements.Add(lit.Value);
        }

        Consume(TokenType.RightBracket);
        return new LiteralExpression(elements.ToArray());
    }

    private Expression ParseExpression(bool allowAtomArgs = true)
    {
        var left = GetExpression(allowAtomArgs);

        while (TryGetOperator(out var op, out var precedence))
        {
            Advance();
            var right = ParseRightOperand(precedence, allowAtomArgs);
            left = new BinaryExpression(left, op, right) { Line = left.Line, Column = left.Column };
        }

        return left;
    }

    private Expression ParseRightOperand(int leftPrecedence, bool allowAtomArgs)
    {
        var right = GetExpression(allowAtomArgs);

        while (TryGetOperator(out var nextOp, out var nextPrecedence) && nextPrecedence > leftPrecedence)
        {
            Advance();
            var nextRight = GetExpression(allowAtomArgs);
            right = new BinaryExpression(right, nextOp, nextRight);
            leftPrecedence = nextPrecedence;
        }

        return right;
    }

    private bool TryGetOperator(out TokenType op, out int precedence)
    {
        op = Current().Type;
        if (OperatorPrecedence.TryGetValue(op, out precedence))
            return true;

        precedence = 0;
        return false;
    }

    private Expression GetExpression(bool allowAtomArgs)
    {
        if (IsLiteralToken(Current().Type))
        {
            var token = Advance();
            return TokenToLiteral(token) with { Line = token.Line, Column = token.Column };
        }

        if (Match(TokenType.LeftBracket))
            return ParseArray();

        if (Check(TokenType.If))
            return ParseIf();

        if (Check(TokenType.For))
            return ParseFor();

        if (Match(TokenType.LeftParen))
        {
            var inner = ParseExpression(true);
            Consume(TokenType.RightParen);
            return inner;
        }

        if (Check(TokenType.Identifier))
            return ParseIdentifierExpression(Advance(), allowAtomArgs);

        throw new Exception($"{Current().Line}: unexpected '{Current().Value}'");
    }

    private Expression ParseIdentifierExpression(Token token, bool allowAtomArgs)
    {
        var (_, name, line, column) = token;

        if (Match(TokenType.Dot))
            return ParseModuleCall(name, line, column);

        if (Match(TokenType.LeftParen))
            return new CallExpression(name, ParseParenArgs()) { Line = line, Column = column };

        if (allowAtomArgs)
        {
            var atomArgs = ParseAtomArgs();
            if (atomArgs.Count > 0)
                return new CallExpression(name, atomArgs) { Line = line, Column = column };
        }

        return new IdentifierExpression(name) { Line = line, Column = column };
    }

    private Expression ParseModuleCall(string name, int line, int column)
    {
        var functionName = Consume(TokenType.Identifier).Value;

        if (Match(TokenType.LeftParen))
            return new ModuleCallExpression(name, functionName, ParseParenArgs()) { Line = line, Column = column };

        return new ModuleCallExpression(name, functionName, ParseAtomArgs()) { Line = line, Column = column };
    }

    private List<Expression> ParseParenArgs()
    {
        var args = new List<Expression>();
        while (!Check(TokenType.RightParen))
            args.Add(ParseExpression(false));
        Consume(TokenType.RightParen);
        return args;
    }

    private static bool IsAtom(TokenType type)
    {
        return IsLiteralToken(type) || AtomStartTokens.Contains(type);
    }

    private static LiteralExpression TokenToLiteral(Token token)
    {
        return token.Type switch
        {
            TokenType.NumberLiteral => new LiteralExpression(ParseNumber(token.Value)),
            TokenType.StringLiteral => new LiteralExpression(token.Value),
            TokenType.BooleanTrueLiteral => new LiteralExpression(true),
            TokenType.BooleanFalseLiteral => new LiteralExpression(false),
            _ => throw new Exception("unreachable")
        };
    }

    private List<Expression> ParseAtomArgs()
    {
        var args = new List<Expression>();
        while (IsAtom(Current().Type))
        {
            if (Match(TokenType.LeftParen))
            {
                var inner = ParseExpression(true);
                Consume(TokenType.RightParen);
                args.Add(inner);
                continue;
            }

            if (Match(TokenType.LeftBracket))
            {
                args.Add(ParseArray());
                continue;
            }

            var token = Advance();
            Expression arg = token.Type == TokenType.Identifier
                ? new IdentifierExpression(token.Value)
                : TokenToLiteral(token);
            args.Add(arg with { Line = token.Line, Column = token.Column });
        }

        return args;
    }
}