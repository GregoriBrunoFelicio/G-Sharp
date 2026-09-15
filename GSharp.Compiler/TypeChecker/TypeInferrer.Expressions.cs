using GSharp.Compiler.AST;
using GSharp.Compiler.Lexer;

namespace GSharp.Compiler.TypeChecker;

public partial class TypeInferrer
{
    // -------------------------------------------------------------------------
    // Literal inference
    // -------------------------------------------------------------------------

    private GsType InferLiteral(LiteralExpression literal)
    {
        return literal.Value switch
        {
            int => new IntType(),
            float => new FloatType(),
            double => new DoubleType(),
            decimal => new DecimalType(),
            string => new StringType(),
            bool => new BoolType(),
            object[] elements => InferArrayLiteral(elements),
            _ => FreshTypeVar()
        };
    }

    private GsType InferArrayLiteral(object[] elements)
    {
        if (elements.Length == 0)
            return new ArrayType(FreshTypeVar());

        var elementType = elements[0] switch
        {
            int => (GsType)new IntType(),
            float => new FloatType(),
            double => new DoubleType(),
            decimal => new DecimalType(),
            string => new StringType(),
            bool => new BoolType(),
            _ => FreshTypeVar()
        };

        return new ArrayType(elementType);
    }

    // -------------------------------------------------------------------------
    // Expression inference
    // -------------------------------------------------------------------------

    private GsType InferBinding(IdentifierExpression binding, TypeEnvironment environment)
    {
        if (environment.TryLookup(binding.Name, out var resolvedType))
            return resolvedType;

        throw new Exception($"{binding.Line}: '{binding.Name}' is not defined");
    }

    private GsType InferBinary(BinaryExpression binary, TypeEnvironment environment)
    {
        var leftType = InferExpression(binary.Left, environment);
        var rightType = InferExpression(binary.Right, environment);

        var isComparisonOperator = binary.Operator is
            TokenType.EqualEqual or TokenType.NotEqual or
            TokenType.LessThan or TokenType.GreaterThan or
            TokenType.LessThanOrEqual or TokenType.GreaterThanOrEqual;

        if (isComparisonOperator)
        {
            _constraints.Add(new TypeConstraint(leftType, rightType, binary.Line, binary.Column));
            return new BoolType();
        }

        var resultType = FreshTypeVar();
        _constraints.Add(new TypeConstraint(leftType, rightType, binary.Line, binary.Column));
        _constraints.Add(new TypeConstraint(resultType, leftType));
        return resultType;
    }

    private GsType InferUnary(UnaryExpression unary, TypeEnvironment environment)
    {
        var operandType = InferExpression(unary.Operand, environment);

        if (unary.Operator == TokenType.Not)
        {
            _constraints.Add(new TypeConstraint(operandType, new BoolType(), unary.Line, unary.Column));
            return new BoolType();
        }

        var resultType = FreshTypeVar();
        _constraints.Add(new TypeConstraint(resultType, operandType));
        return resultType;
    }

    private GsType InferBinding(BindingExpression binding, TypeEnvironment environment)
    {
        var valueType = InferExpression(binding.Value, environment);
        environment.Register(binding.BindingName, valueType);
        return new UnitType();
    }

    private GsType InferPrint(PrintExpression print, TypeEnvironment environment)
    {
        InferExpression(print.Value, environment);
        return new UnitType();
    }

    private GsType InferIf(IfExpression ifExpression, TypeEnvironment environment)
    {
        var conditionType = InferExpression(ifExpression.Condition, environment);
        _constraints.Add(new TypeConstraint(
            conditionType, new BoolType(), ifExpression.Condition.Line, ifExpression.Condition.Column));

        var thenType = InferBody(ifExpression.ThenBody, environment);

        if (ifExpression.ElseBody is null)
            return new UnitType();

        var elseType = InferBody(ifExpression.ElseBody, environment);
        var (line, column) = BodySpan(ifExpression.ElseBody);
        _constraints.Add(new TypeConstraint(thenType, elseType, line, column));
        return thenType;
    }

    private GsType InferFor(ForExpression forExpression, TypeEnvironment environment)
    {
        var iterableType = InferExpression(forExpression.Iterable, environment);
        var elementTypeVar = FreshTypeVar();

        _constraints.Add(new TypeConstraint(iterableType, new ArrayType(elementTypeVar)));

        var bodyEnvironment = environment.CreateChildScope();
        bodyEnvironment.Register(forExpression.BindingName, elementTypeVar);

        var bodyResultType = InferBody(forExpression.Body, bodyEnvironment);
        return new ArrayType(bodyResultType);
    }

    private GsType InferMatch(MatchExpression match, TypeEnvironment environment)
    {
        var scrutineeType = InferExpression(match.Scrutinee, environment);
        GsType? resultType = null;

        foreach (var arm in match.Arms)
        {
            var armEnvironment = environment;

            if (arm.Pattern is IdentifierExpression identifierPattern)
            {
                armEnvironment = environment.CreateChildScope();
                armEnvironment.Register(identifierPattern.Name, scrutineeType);
                // Records the pattern node's type via the normal InferExpression path (which now
                // finds the name just registered above) instead of writing _expressionTypes
                // directly — keeps the single-writer convention every other node goes through,
                // and gives the catch-all name hover for free.
                InferExpression(identifierPattern, armEnvironment);
            }
            else
            {
                var patternType = InferExpression(arm.Pattern, environment);
                _constraints.Add(new TypeConstraint(
                    scrutineeType, patternType, arm.Pattern.Line, arm.Pattern.Column));
            }

            var armBodyType = InferBody(arm.Body, armEnvironment);

            if (resultType is null)
            {
                resultType = armBodyType;
            }
            else
            {
                var (line, column) = BodySpan(arm.Body);
                _constraints.Add(new TypeConstraint(resultType, armBodyType, line, column));
            }
        }

        return resultType!;
    }
}