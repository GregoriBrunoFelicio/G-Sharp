using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using static G.Sharp.Compiler.Parsers.Validations;

namespace G.Sharp.Compiler.Parsers;

public class ExpressionParser(Parser parser)
{
    private readonly ValueParser _valueParser = new(parser);

    public Expression Parse()
    {
        var left = GetExpression();

        while (IsOperator(parser.Current().Type))
        {
            var op = parser.Advance().Type;
            var right = GetExpression();
            left = new BinaryExpression(left, op, right);
        }

        return left;
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

        throw new Exception("Unexpected token in expression.");
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