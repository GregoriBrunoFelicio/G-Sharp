using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Parsers;

public class ValueParser(Parser parser)
{
    public VariableValue Parse(GType type) =>
        type.IsArray
            ? ParseArray(type)
            : GetValue(type);

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

    private VariableValue GetValue(GType type) =>
        type.Kind switch
        {
            GPrimitiveType.Number => ParseNumber(),
            GPrimitiveType.String => ParseString(),
            GPrimitiveType.Boolean => ParseBool(),
            _ => throw new Exception($"Unsupported type: {type.Kind}")
        };

    private ArrayValue ParseArray(GType arrayType)
    {
        // TODO: I really don't like this approach. I need to find a better way to detect when an array is passed directly inside a 'for' loop instead of being assigned to a variable first.
        if (parser.Check(TokenType.LeftBracket))
            parser.Consume(TokenType.LeftBracket);

        var elements = new List<VariableValue>();

        while (!parser.Check(TokenType.RightBracket))
        {
            var element = GetValue(new GType(arrayType.Kind));
            elements.Add(element);
        }

        parser.Consume(TokenType.RightBracket);

        return new ArrayValue(elements, new GType(arrayType.Kind));
    }
}