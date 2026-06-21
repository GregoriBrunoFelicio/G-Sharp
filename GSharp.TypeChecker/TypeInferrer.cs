using GSharp.AST;

namespace GSharp.TypeChecker;

public partial class TypeInferrer
{
    private readonly List<TypeConstraint> _constraints = [];

    private readonly Dictionary<Expression, GsType> _expressionTypes =
        new(ReferenceEqualityComparer.Instance);

    private int _freshTypeVarCounter;

    private TypeVar FreshTypeVar()
    {
        return new TypeVar($"?{_freshTypeVarCounter++}");
    }

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

    private GsType InferExpression(Expression expression, TypeEnvironment environment)
    {
        var inferredType = expression switch
        {
            LiteralExpression literal => InferLiteral(literal),
            IdentifierExpression binding => InferBinding(binding, environment),
            BinaryExpression binary => InferBinary(binary, environment),
            BindingExpression binding => InferBinding(binding, environment),
            PrintExpression print => InferPrint(print, environment),
            IfExpression ifExpression => InferIf(ifExpression, environment),
            ForExpression forExpression => InferFor(forExpression, environment),
            FunctionDeclaration fn => InferFunctionBody(fn, environment),
            CallExpression call => InferCall(call, environment),
            ModuleCallExpression moduleCall => InferModuleCall(moduleCall, environment),
            ImportDeclaration => new UnitType(),
            _ => FreshTypeVar()
        };

        _expressionTypes[expression] = inferredType;

        return inferredType;
    }

    // -------------------------------------------------------------------------
    // Shared helpers
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

    private static (int Line, int Column) BodySpan(List<Expression> body)
    {
        return body.Count > 0 ? (body[^1].Line, body[^1].Column) : (0, 0);
    }

    private TypeEnvironment CreateGlobalEnvironment()
    {
        var globalEnvironment = new TypeEnvironment();

        foreach (var name in BuiltinTypeRules.Keys)
            globalEnvironment.Register(name, FreshTypeVar());

        return globalEnvironment;
    }
}