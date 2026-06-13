using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.TypeChecker;

public partial class TypeInferrer
{
    // -------------------------------------------------------------------------
    // Literal inference
    // -------------------------------------------------------------------------

    private GsType InferLiteral(LiteralExpression literal) => literal.Value switch
    {
        int            => new IntType(),
        float          => new FloatType(),
        double         => new DoubleType(),
        decimal        => new DecimalType(),
        string         => new StringType(),
        bool           => new BoolType(),
        object[] elements => InferArrayLiteral(elements),
        _              => FreshTypeVar()
    };

    private GsType InferArrayLiteral(object[] elements)
    {
        if (elements.Length == 0)
            return new ArrayType(FreshTypeVar());

        var elementType = elements[0] switch
        {
            int     => (GsType)new IntType(),
            float   => new FloatType(),
            double  => new DoubleType(),
            decimal => new DecimalType(),
            string  => new StringType(),
            bool    => new BoolType(),
            _       => FreshTypeVar()
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
        var leftType  = InferExpression(binary.Left, environment);
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

    private GsType InferLet(LetExpression let, TypeEnvironment environment)
    {
        var valueType = InferExpression(let.Value, environment);
        environment.Register(let.BindingName, valueType);
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
        var iterableType   = InferExpression(forExpression.Iterable, environment);
        var elementTypeVar = FreshTypeVar();

        _constraints.Add(new TypeConstraint(iterableType, new ArrayType(elementTypeVar)));

        var bodyEnvironment = environment.CreateChildScope();
        bodyEnvironment.Register(forExpression.BindingName, elementTypeVar);

        var bodyResultType = InferBody(forExpression.Body, bodyEnvironment);
        return new ArrayType(bodyResultType);
    }
}
