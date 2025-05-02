using GSharp.AST;
using GSharp.Lexer;
using static GSharp.Parser.Validations;

namespace GSharp.Parser;

public class ExpressionParser(Parser parser)
{
    private readonly ValueParser _valueParser = new(parser);

    public Expression Parse()
    {
        var left = GetExpression();

        while (TryGetOperator(out var op, out var precedence))
        {
            parser.Advance();

            var right = GetExpression();

            while (TryGetOperator(out var nextOp, out var nextPrecedence) &&
                   nextPrecedence > precedence)
            {
                parser.Advance();
                var nextRight = GetExpression();

                right = new BinaryExpression(right, nextOp, nextRight);
                precedence = nextPrecedence;
            }

            left = new BinaryExpression(left, op, right);
        }

        return left;
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
        if (parser.Check(TokenType.NumberLiteral))
            return new LiteralExpression(_valueParser.Parse(new GType(GPrimitiveType.Number)));

        if (parser.Check(TokenType.StringLiteral))
            return new LiteralExpression(_valueParser.Parse(new GType(GPrimitiveType.String)));

        if (parser.Check(TokenType.BooleanTrueLiteral) || parser.Check(TokenType.BooleanFalseLiteral))
            return new LiteralExpression(_valueParser.Parse(new GType(GPrimitiveType.Boolean)));

        if (parser.Match(TokenType.LeftBracket))
            return ParseArrayExpression(parser, _valueParser);

        if (parser.Match(TokenType.Identifier))
            return new VariableExpression(parser.Previous().Value);

        throw new Exception($"Unexpected token in expression: {parser.Current().Type}.");
    }

    private static LiteralExpression ParseArrayExpression(Parser parser, ValueParser valueParser)
    {
        if (parser.Check(TokenType.RightBracket))
            throw new Exception("Empty arrays are not supported.");

        var token = parser.Current();

        var kind = token.Type switch
        {
            TokenType.StringLiteral => GPrimitiveType.String,
            TokenType.BooleanTrueLiteral or TokenType.BooleanFalseLiteral => GPrimitiveType.Boolean,
            TokenType.NumberLiteral => GPrimitiveType.Number,
            _ => throw new Exception("Unable to infer array element type.")
        };

        var arrayValue = valueParser.Parse(new GType(kind, isArray: true));
        return new LiteralExpression(arrayValue);
    }
}