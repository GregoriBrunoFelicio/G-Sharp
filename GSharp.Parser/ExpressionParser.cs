using System.Globalization;
using GSharp.AST;
using GSharp.Lexer;
using static GSharp.Parser.Validations;

namespace GSharp.Parser;

public class ExpressionParser(Parser parser)
{
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
        if (parser.Match(TokenType.NumberLiteral))
            return new LiteralExpression(ParseNumber(parser.Previous().Value));

        if (parser.Match(TokenType.StringLiteral))
            return new LiteralExpression(parser.Previous().Value);

        if (parser.Match(TokenType.BooleanTrueLiteral))
            return new LiteralExpression(true);

        if (parser.Match(TokenType.BooleanFalseLiteral))
            return new LiteralExpression(false);
        
        if (parser.Match(TokenType.LeftBracket))
            return ParseArrayExpression(parser);

        if (parser.Match(TokenType.Identifier))
            return new VariableExpression(parser.Previous().Value);

        throw new Exception($"Unexpected token in expression: {parser.Current().Type}.");
    }

    private static object ParseNumber(string text)
    {
        if (text.EndsWith('f'))
            return float.Parse(text[..^1]);
    
        if (text.EndsWith('d'))
            return double.Parse(text[..^1]);
    
        if (text.EndsWith('m'))
            return decimal.Parse(text[..^1]);
    
        if (text.Contains('.'))
            return double.Parse(text);
    
        return int.Parse(text);
    }

    private static LiteralExpression ParseArrayExpression(Parser parser)
    {
        var elements = new List<object>();

        if (parser.Check(TokenType.RightBracket))
            throw new Exception("Empty arrays are not supported."); // Maybe not..
        
        Type? elementType = null;

        while (!parser.Check(TokenType.RightBracket))
        {
            var expr = new ExpressionParser(parser).Parse();

            if (expr is not LiteralExpression lit)
                throw new Exception("Only literal expressions are supported in arrays for now.");
            
            var value = lit.Value;

            elementType ??= value?.GetType();

            if (value?.GetType() != elementType)
                throw new Exception(
                    "Array literals must contain elements of the same type."); // o___O

            elements.Add(lit.Value);
        }
        
        parser.Consume(TokenType.RightBracket);

        return new LiteralExpression(elements.ToArray());
    }
}