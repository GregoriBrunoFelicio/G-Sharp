using GSharp.Compiler.AST;
using GSharp.Compiler.Lexer;

namespace GSharp.Compiler.Optimizer;

// Evaluates literal-only expressions (e.g. `1 + 1`, `2 * 3 == 6`) at compile time and replaces
// them with the resulting LiteralExpression. Runs on the raw AST, before TypeInferrer, so it
// deliberately does not depend on GSharp.Compiler.CodeGen.Helpers.RuntimeHelpers (that's runtime
// support reachable only from emitted IL) — the arithmetic/promotion semantics below are a
// from-scratch replication of RuntimeHelpers, kept in lockstep with it by hand.
public static class ConstantFolder
{
    public static List<Expression> FoldAll(List<Expression> expressions) =>
        FoldBody(expressions);

    private static List<Expression> FoldBody(List<Expression> body) =>
        body.Select(Fold).ToList();

    private static Expression Fold(Expression expression)
    {
        return expression switch
        {
            LiteralExpression => expression,
            IdentifierExpression => expression,
            BinaryExpression binary => FoldBinary(binary),
            UnaryExpression unary => FoldUnary(unary),
            BindingExpression binding => binding with { Value = Fold(binding.Value) },
            PrintExpression print => print with { Value = Fold(print.Value) },
            IfExpression ifExpression => ifExpression with
            {
                Condition = Fold(ifExpression.Condition),
                ThenBody = FoldBody(ifExpression.ThenBody),
                ElseBody = ifExpression.ElseBody is null ? null : FoldBody(ifExpression.ElseBody)
            },
            ForExpression forExpression => forExpression with
            {
                Iterable = Fold(forExpression.Iterable),
                Body = FoldBody(forExpression.Body)
            },
            FunctionDeclaration functionDeclaration => functionDeclaration with
            {
                Body = FoldBody(functionDeclaration.Body)
            },
            LambdaExpression lambda => lambda with { Body = FoldBody(lambda.Body) },
            CallExpression call => call with { Arguments = FoldBody(call.Arguments) },
            ModuleCallExpression moduleCall => moduleCall with { Arguments = FoldBody(moduleCall.Arguments) },
            ImportDeclaration => expression,
            _ => expression
        };
    }

    private static Expression FoldBinary(BinaryExpression binary)
    {
        var left = Fold(binary.Left);

        if (binary.Operator == TokenType.And && left is LiteralExpression { Value: false })
            return new LiteralExpression(false) { Line = binary.Line, Column = binary.Column };

        if (binary.Operator == TokenType.Or && left is LiteralExpression { Value: true })
            return new LiteralExpression(true) { Line = binary.Line, Column = binary.Column };

        var right = Fold(binary.Right);

        if (left is LiteralExpression leftLiteral && right is LiteralExpression rightLiteral)
        {
            var result = TryEvaluate(leftLiteral.Value, binary.Operator, rightLiteral.Value);
            if (result is not null)
                return new LiteralExpression(result) { Line = binary.Line, Column = binary.Column };
        }

        return binary with { Left = left, Right = right };
    }

    private static Expression FoldUnary(UnaryExpression unary)
    {
        var operand = Fold(unary.Operand);

        if (operand is LiteralExpression literal)
        {
            var result = TryEvaluateUnary(unary.Operator, literal.Value);
            if (result is not null)
                return new LiteralExpression(result) { Line = unary.Line, Column = unary.Column };
        }

        return unary with { Operand = operand };
    }

    private static object? TryEvaluateUnary(TokenType op, object value)
    {
        return op switch
        {
            TokenType.Not when value is bool b => !b,
            TokenType.Minus when value is int i => -i,
            TokenType.Minus when value is float f => -f,
            TokenType.Minus when value is double d => -d,
            TokenType.Minus when value is decimal m => -m,
            _ => null
        };
    }

    private static object? TryEvaluate(object left, TokenType op, object right)
    {
        if (op == TokenType.EqualEqual) return left.Equals(right);
        if (op == TokenType.NotEqual) return !left.Equals(right);

        if (op == TokenType.And)
            return left is bool lb && right is bool rb ? lb && rb : null;
        if (op == TokenType.Or)
            return left is bool lb2 && right is bool rb2 ? lb2 || rb2 : null;

        if (op == TokenType.Plus && left is string ls && right is string rs)
            return ls + rs;

        if (!IsNumeric(left) || !IsNumeric(right))
            return null;

        var (promotedLeft, promotedRight) = Promote(left, right);

        return op switch
        {
            TokenType.Plus => Arithmetic(promotedLeft, promotedRight, static (a, b) => a + b, static (a, b) => a + b,
                static (a, b) => a + b, static (a, b) => a + b),
            TokenType.Minus => Arithmetic(promotedLeft, promotedRight, static (a, b) => a - b,
                static (a, b) => a - b, static (a, b) => a - b, static (a, b) => a - b),
            TokenType.Multiply => Arithmetic(promotedLeft, promotedRight, static (a, b) => a * b,
                static (a, b) => a * b, static (a, b) => a * b, static (a, b) => a * b),
            TokenType.Divide => TryDivide(promotedLeft, promotedRight),
            TokenType.GreaterThan => CompareTo(promotedLeft, promotedRight) > 0,
            TokenType.LessThan => CompareTo(promotedLeft, promotedRight) < 0,
            TokenType.GreaterThanOrEqual => CompareTo(promotedLeft, promotedRight) >= 0,
            TokenType.LessThanOrEqual => CompareTo(promotedLeft, promotedRight) <= 0,
            _ => null
        };
    }

    private static bool IsNumeric(object value) => value is int or float or double or decimal;

    // Mirrors RuntimeHelpers.Promote exactly, including the float<->decimal hop through double.
    private static (object left, object right) Promote(object left, object right)
    {
        if (left.GetType() == right.GetType()) return (left, right);

        return (left, right) switch
        {
            (int a, float) => ((float)a, right),
            (float, int b) => (left, (float)b),
            (int a, double) => ((double)a, right),
            (double, int b) => (left, (double)b),
            (int a, decimal) => ((decimal)a, right),
            (decimal, int b) => (left, (decimal)b),
            (float a, double) => ((double)a, right),
            (double, float b) => (left, (double)b),
            (float a, decimal) => ((decimal)(double)a, right),
            (decimal, float b) => (left, (decimal)(double)b),
            (double a, decimal) => ((decimal)a, right),
            (decimal, double b) => (left, (decimal)b),
            _ => throw new Exception($"Incompatible types: {left.GetType().Name} and {right.GetType().Name}")
        };
    }

    private static object Arithmetic(object left, object right, Func<int, int, int> onInt,
        Func<float, float, float> onFloat, Func<double, double, double> onDouble,
        Func<decimal, decimal, decimal> onDecimal)
    {
        return left switch
        {
            int a => onInt(a, (int)right),
            float a => onFloat(a, (float)right),
            double a => onDouble(a, (double)right),
            decimal a => onDecimal(a, (decimal)right),
            _ => throw new Exception($"Incompatible types: {left.GetType().Name} and {right.GetType().Name}")
        };
    }

    // Returns null (don't fold) instead of throwing, so `10 / 0` is left as a BinaryExpression
    // and still throws DivideByZeroException at runtime, exactly as it does today.
    private static object? TryDivide(object left, object right)
    {
        if (left is int && (int)right == 0) return null;
        if (left is decimal && (decimal)right == 0m) return null;

        return Arithmetic(left, right, static (a, b) => a / b, static (a, b) => a / b,
            static (a, b) => a / b, static (a, b) => a / b);
    }

    private static int CompareTo(object left, object right)
    {
        return left switch
        {
            int a => a.CompareTo((int)right),
            float a => a.CompareTo((float)right),
            double a => a.CompareTo((double)right),
            decimal a => a.CompareTo((decimal)right),
            _ => throw new Exception($"Incompatible types: {left.GetType().Name} and {right.GetType().Name}")
        };
    }
}
