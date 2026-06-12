using GSharp.AST;
using GSharp.Lexer;
using static GSharp.Parser.Validations;

namespace GSharp.Parser;

public class ExpressionParser(Parser parser, bool allowAtomArgs = true)
{
    public Expression Parse()
    {
        var left = GetExpression();

        while (TryGetOperator(out var op, out var precedence))
        {
            parser.Advance();
            var right = ParseRightOperand(precedence);
            left = new BinaryExpression(left, op, right) { Line = left.Line, Column = left.Column };
        }

        return left;
    }

    private Expression ParseRightOperand(int leftPrecedence)
    {
        var right = GetExpression();

        while (TryGetOperator(out var nextOp, out var nextPrecedence) && nextPrecedence > leftPrecedence)
        {
            parser.Advance();
            var nextRight = GetExpression();
            right = new BinaryExpression(right, nextOp, nextRight);
            leftPrecedence = nextPrecedence;
        }

        return right;
    }

    private bool TryGetOperator(out TokenType op, out int precedence)
    {
        op = parser.Current().Type;
        if (OperatorPrecedence.TryGetValue(op, out precedence))
            return true;

        precedence = 0;
        return false;
    }

    private Expression GetExpression()
    {
        if (IsLiteralToken(parser.Current().Type))
        {
            var token = parser.Advance();
            return TokenToLiteral(token) with { Line = token.Line, Column = token.Column };
        }

        if (parser.Match(TokenType.LeftBracket))
            return new ArrayParser(parser).Parse();

        if (parser.Check(TokenType.If))
            return new IfParser(parser).Parse();

        if (parser.Check(TokenType.For))
            return new ForParser(parser).Parse();

        if (parser.Match(TokenType.LeftParen))
        {
            var inner = new ExpressionParser(parser, allowAtomArgs: true).Parse();
            parser.Consume(TokenType.RightParen);
            return inner;
        }

        if (parser.Check(TokenType.Identifier))
            return ParseIdentifierExpression(parser.Advance());

        throw new Exception($"{parser.Current().Line}: unexpected '{parser.Current().Value}'");
    }

    private Expression ParseIdentifierExpression(Token token)
    {
        var (_, name, line, column) = token;

        if (parser.Match(TokenType.Dot))
            return ParseQualifiedCall(name, line, column);

        if (parser.Match(TokenType.LeftParen))
            return new CallExpression(name, ParseParenArgs()) { Line = line, Column = column };

        if (allowAtomArgs)
        {
            var atomArgs = ParseAtomArgs();
            if (atomArgs.Count > 0)
                return new CallExpression(name, atomArgs) { Line = line, Column = column };
        }

        return new BindingExpression(name) { Line = line, Column = column };
    }

    private Expression ParseQualifiedCall(string name, int line, int column)
    {
        var functionName = parser.Consume(TokenType.Identifier).Value;

        if (parser.Match(TokenType.LeftParen))
            return new QualifiedCallExpression(name, functionName, ParseParenArgs()) { Line = line, Column = column };

        if (allowAtomArgs)
            return new QualifiedCallExpression(name, functionName, ParseAtomArgs()) { Line = line, Column = column };

        return new QualifiedCallExpression(name, functionName, []) { Line = line, Column = column };
    }

    private List<Expression> ParseParenArgs()
    {
        var args = new List<Expression>();
        while (!parser.Check(TokenType.RightParen))
            args.Add(new ExpressionParser(parser, allowAtomArgs: false).Parse());
        parser.Consume(TokenType.RightParen);
        return args;
    }

    private static bool IsAtom(TokenType type) =>
        IsLiteralToken(type) || type == TokenType.Identifier || type == TokenType.LeftParen;

    private static LiteralExpression TokenToLiteral(Token token) => token.Type switch
    {
        TokenType.NumberLiteral       => new LiteralExpression(ParseNumber(token.Value)),
        TokenType.StringLiteral       => new LiteralExpression(token.Value),
        TokenType.BooleanTrueLiteral  => new LiteralExpression(true),
        TokenType.BooleanFalseLiteral => new LiteralExpression(false),
        _                             => throw new Exception("unreachable"),
    };

    private List<Expression> ParseAtomArgs()
    {
        var args = new List<Expression>();
        while (IsAtom(parser.Current().Type))
        {
            if (parser.Match(TokenType.LeftParen))
            {
                var inner = new ExpressionParser(parser, allowAtomArgs: true).Parse();
                parser.Consume(TokenType.RightParen);
                args.Add(inner);
                continue;
            }

            var token = parser.Advance();
            Expression arg = token.Type == TokenType.Identifier
                ? new BindingExpression(token.Value)
                : TokenToLiteral(token);
            args.Add(arg with { Line = token.Line, Column = token.Column });
        }
        return args;
    }
}
