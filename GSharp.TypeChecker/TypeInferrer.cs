using GSharp.AST;
using GSharp.Lexer;
using GSharp.Stdlib;

namespace GSharp.TypeChecker;

public class TypeInferrer
{
    private int _freshTypeVarCounter = 0;
    private readonly List<TypeConstraint> _constraints = [];
    private readonly Dictionary<Expression, GsType> _expressionTypes =
        new(ReferenceEqualityComparer.Instance);

    private TypeVar FreshTypeVar() => new TypeVar($"?{_freshTypeVarCounter++}");

    public Dictionary<Expression, GsType> Infer(List<Expression> expressions)
    {
        var globalEnvironment = CreateGlobalEnvironment();

        foreach (var fn in expressions.OfType<FunctionDeclaration>())
            RegisterFunctionSignature(fn, globalEnvironment);

        foreach (var expression in expressions)
            InferExpression(expression, globalEnvironment);

        var substitution = Unifier.Unify(_constraints);
        var resolvedTypes = new Dictionary<Expression, GsType>(ReferenceEqualityComparer.Instance);

        foreach (var (expression, inferredType) in _expressionTypes)
            resolvedTypes[expression] = substitution.Apply(inferredType);

        return resolvedTypes;
    }

    private void RegisterFunctionSignature(FunctionDeclaration fn, TypeEnvironment environment)
    {
        var parameterTypeVars = fn.Parameters.Select(_ => FreshTypeVar()).ToList();
        var returnTypeVar = FreshTypeVar();
        var functionType = BuildCurriedFunctionType(parameterTypeVars, returnTypeVar);
        environment.Register(fn.Name, functionType);
    }

    private GsType InferExpression(Expression expression, TypeEnvironment environment)
    {
        var inferredType = expression switch
        {
            LiteralExpression literal => InferLiteral(literal),
            BindingExpression binding => InferBinding(binding, environment),
            BinaryExpression binary => InferBinary(binary, environment),
            LetExpression let => InferLet(let, environment),
            PrintExpression print => InferPrint(print, environment),
            IfExpression ifExpression => InferIf(ifExpression, environment),
            ForExpression forExpression => InferFor(forExpression, environment),
            FunctionDeclaration fn => InferFunctionBody(fn, environment),
            CallExpression call => InferCall(call, environment),
            QualifiedCallExpression qualified => InferQualifiedCall(qualified, environment),
            ImportDeclaration => new UnitType(),
            _ => FreshTypeVar()
        };

        _expressionTypes[expression] = inferredType;
        return inferredType;
    }

    // -------------------------------------------------------------------------
    // Literal inference
    // -------------------------------------------------------------------------

    private GsType InferLiteral(LiteralExpression literal) => literal.Value switch
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

    private GsType InferBinding(BindingExpression binding, TypeEnvironment environment)
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

    private GsType InferLet(LetExpression let, TypeEnvironment environment)
    {
        var valueType = InferExpression(let.Value, environment);
        environment.Register(let.BindingName, valueType);
        return valueType;
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

    // -------------------------------------------------------------------------
    // Function inference
    // -------------------------------------------------------------------------

    private GsType InferFunctionBody(FunctionDeclaration fn, TypeEnvironment environment)
    {
        var registeredFunctionType = environment.Lookup(fn.Name);

        var bodyEnvironment = environment.CreateChildScope();
        var parameterTypeVars = ExtractParameterTypeVars(registeredFunctionType, fn.Parameters.Count);

        for (var i = 0; i < fn.Parameters.Count; i++)
            bodyEnvironment.Register(fn.Parameters[i], parameterTypeVars[i]);

        var bodyResultType = InferBody(fn.Body, bodyEnvironment);
        var returnTypeVar = ExtractReturnTypeVar(registeredFunctionType, fn.Parameters.Count);
        _constraints.Add(new TypeConstraint(returnTypeVar, bodyResultType));

        return registeredFunctionType;
    }

    private GsType InferCall(CallExpression call, TypeEnvironment environment)
    {
        if (!environment.TryLookup(call.Callee, out var calleeType))
            throw new Exception($"unknown function '{call.Callee}'");

        return ApplyArguments(calleeType, call.Arguments, environment);
    }

    private GsType InferQualifiedCall(QualifiedCallExpression qualified, TypeEnvironment environment)
    {
        var qualifiedName = $"{qualified.Module}.{qualified.Function}";

        if (BuiltinCatalog.All.ContainsKey(qualifiedName))
            return InferBuiltinCall(qualifiedName, qualified.Arguments, environment);

        if (!environment.TryLookup(qualifiedName, out var calleeType))
            calleeType = FreshTypeVar();

        return ApplyArguments(calleeType, qualified.Arguments, environment);
    }

    private GsType ApplyArguments(GsType calleeType, List<Expression> arguments, TypeEnvironment environment)
    {
        var currentType = calleeType;

        foreach (var argument in arguments)
        {
            var argumentType = InferExpression(argument, environment);
            var returnTypeVar = FreshTypeVar();
            _constraints.Add(new TypeConstraint(currentType, new FunctionType(argumentType, returnTypeVar)));
            currentType = returnTypeVar;
        }

        return currentType;
    }

    // -------------------------------------------------------------------------
    // Builtin functions
    // -------------------------------------------------------------------------

    private GsType InferBuiltinCall(string name, List<Expression> arguments, TypeEnvironment environment)
    {
        if (BuiltinCatalog.All.TryGetValue(name, out var expected) && arguments.Count != expected)
        {
            var argWord = expected == 1 ? "argument" : "arguments";
            throw new Exception($"'{name}' expects {expected} {argWord} but got {arguments.Count}");
        }

        var elementTypeVar = FreshTypeVar();
        var arrayType = new ArrayType(elementTypeVar);

        return name switch
        {
            "array.head" or "array.last" =>
                InferSingleArrayArg(arguments, arrayType, elementTypeVar, environment),
            "array.tail" or "array.reverse" or "array.sort" =>
                InferSingleArrayArg(arguments, arrayType, arrayType, environment),
            "array.len" =>
                InferSingleArrayArg(arguments, arrayType, new IntType(), environment),
            "array.empty" =>
                InferSingleArrayArg(arguments, arrayType, new BoolType(), environment),
            "array.concat" =>
                InferConcat(arguments, environment),
            "string.from" =>
                InferStringFrom(arguments, environment),
            _ => FreshTypeVar()
        };
    }

    private GsType InferSingleArrayArg(
        List<Expression> arguments,
        ArrayType expectedArgType,
        GsType resultType,
        TypeEnvironment environment)
    {
        if (arguments.Count > 0)
        {
            var argType = InferExpression(arguments[0], environment);
            _constraints.Add(new TypeConstraint(argType, expectedArgType));
        }
        return resultType;
    }

    private GsType InferConcat(List<Expression> arguments, TypeEnvironment environment)
    {
        foreach (var argument in arguments)
            InferExpression(argument, environment);
        return new ArrayType(FreshTypeVar());
    }

    private GsType InferStringFrom(List<Expression> arguments, TypeEnvironment environment)
    {
        if (arguments.Count > 0)
            InferExpression(arguments[0], environment);
        return new StringType();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private GsType InferBody(List<Expression> body, TypeEnvironment environment)
    {
        if (body.Count == 0)
            return new UnitType();

        GsType lastType = new UnitType();
        foreach (var expression in body)
            lastType = InferExpression(expression, environment);

        return lastType;
    }

    private static (int Line, int Column) BodySpan(List<Expression> body) =>
        body.Count > 0 ? (body[^1].Line, body[^1].Column) : (0, 0);

    private static GsType BuildCurriedFunctionType(List<TypeVar> parameterTypeVars, TypeVar returnTypeVar)
    {
        GsType result = returnTypeVar;
        for (var i = parameterTypeVars.Count - 1; i >= 0; i--)
            result = new FunctionType(parameterTypeVars[i], result);
        return result;
    }

    private static List<GsType> ExtractParameterTypeVars(GsType functionType, int parameterCount)
    {
        var parameterTypes = new List<GsType>();
        var currentType = functionType;

        for (var i = 0; i < parameterCount; i++)
        {
            if (currentType is FunctionType ft)
            {
                parameterTypes.Add(ft.ParameterType);
                currentType = ft.ReturnType;
            }
        }

        return parameterTypes;
    }

    private static GsType ExtractReturnTypeVar(GsType functionType, int parameterCount)
    {
        var currentType = functionType;
        for (var i = 0; i < parameterCount; i++)
            if (currentType is FunctionType ft)
                currentType = ft.ReturnType;
        return currentType;
    }

    private TypeEnvironment CreateGlobalEnvironment()
    {
        var globalEnvironment = new TypeEnvironment();

        foreach (var name in BuiltinCatalog.All.Keys)
            globalEnvironment.Register(name, FreshTypeVar());

        return globalEnvironment;
    }
}
