using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Parsers;

public class ValueParser(Parser parser)
{
    public VariableValue Parse(GType type) =>
        type switch
        {
            GType.String => new StringValue(parser.Consume(TokenType.StringLiteral).Value),
            GType.Number => ParseNumber(),
            GType.Boolean => ParseBool(),
            _ => throw new Exception("Unsupported type")
        };

    private NumberValue ParseNumber()
    {
        var token = parser.Consume(TokenType.NumberLiteral).Value;

        if (token.EndsWith('f')) return new FloatValue(float.Parse(token[..^1]));
        if (token.EndsWith('d')) return new DoubleValue(double.Parse(token[..^1]));
        if (token.EndsWith('m')) return new DecimalValue(decimal.Parse(token[..^1]));

        return new IntValue(int.Parse(token));
    }

    private BooleanValue ParseBool()
    {
        if (parser.Match(TokenType.BooleanTrueLiteral)) return new BooleanValue(true);
        if (parser.Match(TokenType.BooleanFalseLiteral)) return new BooleanValue(false);
        throw new Exception("Expected boolean literal");
    }
}