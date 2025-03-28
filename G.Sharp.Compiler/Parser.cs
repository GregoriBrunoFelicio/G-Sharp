using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using Type = G.Sharp.Compiler.AST.Type;

namespace G.Sharp.Compiler;

public class Parser(IEnumerable<Token> tokens)
{
    private readonly List<Token> _tokens = tokens.ToList();
    private int _current;

    public List<Statement> Parse()
    {
        var statements = new List<Statement>();

        while (IsNotEndOfFile())
        {
            if (Match(TokenType.Let))
            {
                var letStatement = HandleLetStatement();
                statements.Add(letStatement);
                continue;
            }
            if (Match(TokenType.Println))
            {
                var printStatement = HandlePrintStatement();
                statements.Add(printStatement);
                continue;
            }

            throw new Exception("Invalid statement");
        }

        return statements;
    }

    private LetStatement HandleLetStatement()
    {
        var name = Consume(TokenType.Identifier).Value;

        Consume(TokenType.Colon);

        var varType = GetGSharpType();

        Consume(TokenType.Equals);

        var value = GetValue(varType);

        Consume(TokenType.Semicolon);

        return new LetStatement(name, value);
    }

    private PrintStatement HandlePrintStatement()
    {
        var name = Consume(TokenType.Identifier).Value;
        Consume(TokenType.Semicolon);
        return new PrintStatement(name);
    }

    private VariableValue GetValue(Type type) =>
        type switch
        {
            Type.Number => new NumberValue(int.Parse(Consume(TokenType.NumberLiteral).Value)),
            Type.String => new StringValue(Consume(TokenType.StringLiteral).Value),
            Type.Boolean => new BooleanValue(ConsumeBool()),
            _ => throw new Exception("Unsupported type")
        };

    private bool ConsumeBool()
    {
        if (Match(TokenType.BooleanTrueLiteral)) return true;
        if (Match(TokenType.BooleanFalseLiteral)) return false;
        throw new Exception("Expected boolean literal");
    }
    
    private Type GetGSharpType()
    {
        if (Match(TokenType.Number))
        {
            return Type.Number;
        }

        if (Match(TokenType.String))
        {
            return Type.String;
        }

        if (Match(TokenType.Boolean))
        {
            return Type.Boolean;
        }

        throw new Exception("Expected a type not found.");
    }
    

    private bool IsNotEndOfFile() =>
        _current < _tokens.Count && _tokens[_current].Type != TokenType.EndOfFile;

    private Token Consume(TokenType type)
    {
        if (Check(type))
            return Advance();
        throw new Exception($"Expected token {type}, got {_tokens[_current].Type}");
    }

    private bool Match(TokenType type)
    {
        if (!Check(type)) return false;
        Advance();
        return true;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return _tokens[_current].Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return _tokens[_current - 1];
    }

    private bool IsAtEnd() => _current >= _tokens.Count;
}