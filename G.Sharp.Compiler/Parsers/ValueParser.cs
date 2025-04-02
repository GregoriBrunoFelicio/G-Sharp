using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;

namespace G.Sharp.Compiler.Parsers;

public class ValueParser(Parser parser)
{
    public VariableValue Parse(GType type) =>
        type.IsArray
            ? ParseArray(type)
            : GetValue(type);

    private StringValue ParseString() =>
        new(parser.Consume(TokenType.StringLiteral).Value);

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

    private VariableValue GetValue(GType type) =>
        type.Kind switch
        {
            GPrimitiveType.String => ParseString(),
            GPrimitiveType.Number => ParseNumber(),
            GPrimitiveType.Boolean => ParseBool(),
            _ => throw new Exception("Unsupported type")
        };

    private ArrayValue ParseArray(GType arrayType)
    {
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