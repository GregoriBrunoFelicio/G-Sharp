using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Parsers;

public class ExpressionParser(Parser parser)
{
    private readonly ValueParser _valueParser = new(parser);

    public Expression Parse()
    {
        if (parser.Check(TokenType.NumberLiteral))
            return new LiteralExpression(_valueParser.Parse(new GType(GPrimitiveType.Number)));

        if (parser.Check(TokenType.StringLiteral))
            return new LiteralExpression(_valueParser.Parse(new GType(GPrimitiveType.String)));

        if (parser.Check(TokenType.BooleanTrueLiteral) || parser.Check(TokenType.BooleanFalseLiteral))
            return new LiteralExpression(_valueParser.Parse(new GType(GPrimitiveType.Boolean)));

        if (parser.Match(TokenType.LeftBracket))
        {
            return ParseArrayExpression();
        }

        if (parser.Match(TokenType.Identifier))
            return new VariableExpression(parser.Previous().Value);

        throw new Exception("Unexpected token in expression.");
    }

    private LiteralExpression ParseArrayExpression()
    {
        if (parser.Check(TokenType.RightBracket))
            throw new Exception("Empty arrays are not supported.");

        var token = parser.Current();

        var kind = token.Type switch
        {
            TokenType.StringLiteral => GPrimitiveType.String,
            TokenType.BooleanTrueLiteral or TokenType.BooleanFalseLiteral => GPrimitiveType.Boolean,
            TokenType.NumberLiteral => InferNumberKind(token.Value),
            _ => throw new Exception("Unable to infer array element type.")
        };

        var arrayValue = _valueParser.Parse(new GType(kind, isArray: true));
        return new LiteralExpression(arrayValue);
    }

    private GPrimitiveType InferNumberKind(string raw)
    {
        if (raw.Length == 0) return GPrimitiveType.Int;

        return raw[^1] switch
        {
            'f' => GPrimitiveType.Float,
            'd' => GPrimitiveType.Double,
            'm' => GPrimitiveType.Decimal,
            _ => GPrimitiveType.Int
        };
    }
}