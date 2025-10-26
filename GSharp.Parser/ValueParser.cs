using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class ValueParser(Parser parser)
{
    public VariableValue Parse(GType type)
    {
        if (type is GArrayType arrayType)
            return ParseArray(arrayType.ElementType);

        return GetValue(type);
    }

    private VariableValue GetValue(GType type) =>
        type switch
        {
            GNumberType => ParseNumber(),
            GStringType => ParseString(),
            GBooleanType => ParseBool(),
            _ => throw new Exception($"Unsupported type: {type.Name}")
        };

    private NumberValue ParseNumber()
    {
        var token = parser.Consume(TokenType.NumberLiteral).Value;

        if (token.EndsWith('f')) return new FloatValue(float.Parse(token[..^1]));
        if (token.EndsWith('d')) return new DoubleValue(double.Parse(token[..^1]));
        if (token.EndsWith('m')) return new DecimalValue(decimal.Parse(token[..^1]));

        if (token.Contains('.'))
            throw new Exception("Numeric literals with decimal points must specify a type suffix (f, d, or m).");

        return new IntValue(int.Parse(token));
    }

    private StringValue ParseString() =>
        new(parser.Consume(TokenType.StringLiteral).Value);

    private BooleanValue ParseBool()
    {
        if (parser.Match(TokenType.BooleanTrueLiteral)) return new BooleanValue(true);
        if (parser.Match(TokenType.BooleanFalseLiteral)) return new BooleanValue(false);
        throw new Exception("Expected boolean literal");
    }

    private ArrayValue ParseArray(GType elementType)
    {
        // TODO: I really don't like this approach. I need to find a better way to detect when an array is passed directly inside a 'for' loop instead of being assigned to a variable first.
        if (parser.Check(TokenType.LeftBracket))
            parser.Consume(TokenType.LeftBracket);

        var elements = new List<VariableValue>();

        while (!parser.Check(TokenType.RightBracket))
        {
            var element = GetValue(elementType);
            elements.Add(element);
        }

        parser.Consume(TokenType.RightBracket);

        return new ArrayValue(elements, elementType);
    }
}