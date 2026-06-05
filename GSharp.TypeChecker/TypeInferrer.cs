using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.TypeChecker;

/// <summary>
/// Walks the AST, assigns a type to each expression, generates equality constraints,
/// and returns the resolved types after unification.
///
/// The inference works in two passes (mirrors the FunctionEmitter two-pass approach):
///   Pass 1 — register all function declarations with fresh TypeVars.
///            This allows recursive calls to resolve: factorial can reference itself
///            before its body is fully inferred.
///   Pass 2 — infer each top-level expression, generating constraints along the way.
///
/// After both passes, all collected constraints are sent to the Unifier.
/// The Unifier resolves all TypeVars to concrete types and returns a Substitution.
/// The Substitution is applied to every expression's type to produce the final result.
///
/// Current limitation — monomorphic functions:
///   A function like `identity x => x` gets one concrete type per program.
///   If called with both Int and String, it fails with a type mismatch.
///   Full let-polymorphism (∀a. a → a) where each call site gets a fresh type
///   is the next step in the H-M implementation.
/// </summary>
public class TypeInferrer
{
    private int _freshTypeVarCounter = 0;
    private readonly List<TypeConstraint> _constraints = new();
    private readonly Dictionary<Expression, GsType> _expressionTypes =
        new(ReferenceEqualityComparer.Instance);

    private TypeVar FreshTypeVar() => new TypeVar($"?{_freshTypeVarCounter++}");

    public Dictionary<Expression, GsType> Infer(List<Expression> expressions)
    {
        var globalEnvironment = CreateGlobalEnvironment();

        // Pass 1 — register function names before inferring bodies (supports recursion)
        foreach (var fn in expressions.OfType<FunctionDeclaration>())
            RegisterFunctionSignature(fn, globalEnvironment);

        // Pass 2 — infer all top-level expressions
        foreach (var expression in expressions)
            InferExpression(expression, globalEnvironment);

        var substitution  = Unifier.Unify(_constraints);
        var resolvedTypes = new Dictionary<Expression, GsType>(ReferenceEqualityComparer.Instance);

        foreach (var (expression, inferredType) in _expressionTypes)
            resolvedTypes[expression] = substitution.Apply(inferredType);

        return resolvedTypes;
    }

    // Registers a function's type signature using fresh TypeVars for each parameter.
    // Builds a curried FunctionType: param1 → param2 → ... → returnType.
    private void RegisterFunctionSignature(FunctionDeclaration fn, TypeEnvironment environment)
    {
        var parameterTypeVars = fn.Parameters.Select(_ => FreshTypeVar()).ToList();
        var returnTypeVar     = FreshTypeVar();

        var functionType = BuildCurriedFunctionType(parameterTypeVars, returnTypeVar);
        environment.Register(fn.Name, functionType);
    }

    // Infers the type of a single expression and records it in _expressionTypes.
    private GsType InferExpression(Expression expression, TypeEnvironment environment)
    {
        var inferredType = expression switch
        {
            LiteralExpression literal          => InferLiteral(literal),
            BindingExpression binding          => InferBinding(binding, environment),
            BinaryExpression binary            => InferBinary(binary, environment),
            LetExpression let                  => InferLet(let, environment),
            PrintExpression print              => InferPrint(print, environment),
            IfExpression ifExpression          => InferIf(ifExpression, environment),
            ForExpression forExpression        => InferFor(forExpression, environment),
            FunctionDeclaration fn             => InferFunctionBody(fn, environment),
            CallExpression call                => InferCall(call, environment),
            QualifiedCallExpression qualified  => InferQualifiedCall(qualified, environment),
            ImportDeclaration                  => new UnitType(),
            DotnetImportDeclaration            => new UnitType(),
            _                                  => FreshTypeVar()
        };

        _expressionTypes[expression] = inferredType;
        return inferredType;
    }

    // -------------------------------------------------------------------------
    // Literal inference
    // -------------------------------------------------------------------------

    private GsType InferLiteral(LiteralExpression literal) => literal.Value switch
    {
        int     => new IntType(),
        float   => new FloatType(),
        double  => new DoubleType(),
        decimal => new DecimalType(),
        string  => new StringType(),
        bool    => new BoolType(),
        object[] elements => InferArrayLiteral(elements),
        _       => FreshTypeVar()
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

    private GsType InferBinding(BindingExpression binding, TypeEnvironment environment)
    {
        if (environment.TryLookup(binding.Name, out var resolvedType))
            return resolvedType;

        // Unbound name. Previously this invented a fresh TypeVar so inference could
        // continue, which pushed "undefined name" detection all the way to CodeGen —
        // out of reach of the LSP. Reporting it here (with the binding's source line)
        // lets the language server surface it as a diagnostic on the right line.
        throw new Exception($"{binding.Line}: '{binding.Name}' is not defined");
    }

    private GsType InferBinary(BinaryExpression binary, TypeEnvironment environment)
    {
        var leftType  = InferExpression(binary.Left,  environment);
        var rightType = InferExpression(binary.Right, environment);

        var isComparisonOperator = binary.Operator is
            TokenType.EqualEqual or TokenType.NotEqual or
            TokenType.LessThan   or TokenType.GreaterThan or
            TokenType.LessThanOrEqual or TokenType.GreaterThanOrEqual;

        if (isComparisonOperator)
        {
            _constraints.Add(new TypeConstraint(leftType, rightType, binary.Line, binary.Column));
            return new BoolType();
        }

        // Arithmetic: both sides must be the same numeric type, result is that type
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

        // Point a branch mismatch at the else branch's value (where the divergence shows up).
        var (line, column) = BodySpan(ifExpression.ElseBody);
        _constraints.Add(new TypeConstraint(thenType, elseType, line, column));
        return thenType;
    }

    private GsType InferFor(ForExpression forExpression, TypeEnvironment environment)
    {
        var iterableType    = InferExpression(forExpression.Iterable, environment);
        var elementTypeVar  = FreshTypeVar();

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
        // Retrieve the already-registered function type from Pass 1
        var registeredFunctionType = environment.Lookup(fn.Name);

        // Create a child scope with each parameter bound to its TypeVar
        var bodyEnvironment    = environment.CreateChildScope();
        var parameterTypeVars  = ExtractParameterTypeVars(registeredFunctionType, fn.Parameters.Count);

        for (var i = 0; i < fn.Parameters.Count; i++)
            bodyEnvironment.Register(fn.Parameters[i], parameterTypeVars[i]);

        // Infer the body — the last expression is the return value
        var bodyResultType = InferBody(fn.Body, bodyEnvironment);

        // Constrain the return TypeVar to the inferred body type
        var returnTypeVar = ExtractReturnTypeVar(registeredFunctionType, fn.Parameters.Count);
        _constraints.Add(new TypeConstraint(returnTypeVar, bodyResultType));

        return registeredFunctionType;
    }

    private GsType InferCall(CallExpression call, TypeEnvironment environment)
    {
        if (IsPrecompiled(call.Callee))
            return InferBuiltInCall(call, environment);

        if (!environment.TryLookup(call.Callee, out var calleeType))
            throw new Exception($"unknown function '{call.Callee}'");

        return ApplyArguments(calleeType, call.Arguments, environment);
    }

    private GsType InferQualifiedCall(QualifiedCallExpression qualified, TypeEnvironment environment)
    {
        var qualifiedName = $"{qualified.Module}.{qualified.Function}";

        if (!environment.TryLookup(qualifiedName, out var calleeType))
        {
            // Module function not registered — assign a fresh type for now
            calleeType = FreshTypeVar();
        }

        return ApplyArguments(calleeType, qualified.Arguments, environment);
    }

    // Applies a list of arguments to a function type, returning the final result type.
    // For `add 3 5` where add : Int → Int → Int:
    //   apply 3  → result: Int → Int
    //   apply 5  → result: Int
    private GsType ApplyArguments(GsType calleeType, List<Expression> arguments, TypeEnvironment environment)
    {
        var currentType = calleeType;

        foreach (var argument in arguments)
        {
            var argumentType  = InferExpression(argument, environment);
            var returnTypeVar = FreshTypeVar();

            _constraints.Add(new TypeConstraint(currentType, new FunctionType(argumentType, returnTypeVar)));
            currentType = returnTypeVar;
        }

        return currentType;
    }

    // -------------------------------------------------------------------------
    // Built-in functions
    // -------------------------------------------------------------------------

    private static bool IsPrecompiled(string name) => PrecompiledCatalog.Functions.ContainsKey(name);

    private static void ValidatePrecompiledArguments(CallExpression call)
    {
        if (!PrecompiledCatalog.Functions.TryGetValue(call.Callee, out var expected))
            return;

        var got = call.Arguments.Count;
        if (got != expected)
        {
            var argWord = expected == 1 ? "argument" : "arguments";
            throw new Exception($"'{call.Callee}' expects {expected} {argWord} but got {got}");
        }
    }

    private GsType InferBuiltInCall(CallExpression call, TypeEnvironment environment)
    {
        ValidatePrecompiledArguments(call);

        var elementTypeVar = FreshTypeVar();
        var arrayType      = new ArrayType(elementTypeVar);

        return call.Callee switch
        {
            "head" or "last" => InferSingleArrayArg(call, arrayType, elementTypeVar, environment),
            "tail"           => InferSingleArrayArg(call, arrayType, arrayType,      environment),
            "reverse"        => InferSingleArrayArg(call, arrayType, arrayType,      environment),
            "len"            => InferSingleArrayArg(call, arrayType, new IntType(),  environment),
            "empty"          => InferSingleArrayArg(call, arrayType, new BoolType(), environment),
            "nth"            => InferNth(call, arrayType, elementTypeVar, environment),
            "concat"         => InferConcat(call, arrayType, environment),
            "str"            => InferStr(call, environment),
            _                => FreshTypeVar()
        };
    }

    private GsType InferSingleArrayArg(
        CallExpression call,
        ArrayType expectedArgType,
        GsType resultType,
        TypeEnvironment environment)
    {
        if (call.Arguments.Count > 0)
        {
            var argType = InferExpression(call.Arguments[0], environment);
            _constraints.Add(new TypeConstraint(argType, expectedArgType));
        }
        return resultType;
    }

    private GsType InferNth(CallExpression call, ArrayType arrayType, GsType elementTypeVar, TypeEnvironment environment)
    {
        if (call.Arguments.Count >= 1)
        {
            var argType = InferExpression(call.Arguments[0], environment);
            _constraints.Add(new TypeConstraint(argType, arrayType));
        }
        if (call.Arguments.Count >= 2)
        {
            var indexType = InferExpression(call.Arguments[1], environment);
            _constraints.Add(new TypeConstraint(indexType, new IntType()));
        }
        return elementTypeVar;
    }

    private GsType InferConcat(CallExpression call, ArrayType arrayType, TypeEnvironment environment)
    {
        foreach (var argument in call.Arguments)
            InferExpression(argument, environment);

        return new ArrayType(FreshTypeVar());
    }

    private GsType InferStr(CallExpression call, TypeEnvironment environment)
    {
        if (call.Arguments.Count > 0)
            InferExpression(call.Arguments[0], environment);

        return new StringType();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    // Infers the type of a body (list of expressions). Returns the type of the last expression.
    // All expressions before the last have their values discarded.
    private GsType InferBody(List<Expression> body, TypeEnvironment environment)
    {
        if (body.Count == 0)
            return new UnitType();

        GsType lastType = new UnitType();
        foreach (var expression in body)
            lastType = InferExpression(expression, environment);

        return lastType;
    }

    // Source position of a body's value (its last expression), used to anchor branch
    // mismatch errors. Falls back to (0, 0) — "unknown" — for an empty body.
    private static (int Line, int Column) BodySpan(List<Expression> body) =>
        body.Count > 0 ? (body[^1].Line, body[^1].Column) : (0, 0);

    // Builds a curried FunctionType from a list of parameter TypeVars and a return TypeVar.
    // [?a, ?b] with return ?r → FunctionType(?a, FunctionType(?b, ?r))
    private static GsType BuildCurriedFunctionType(List<TypeVar> parameterTypeVars, TypeVar returnTypeVar)
    {
        GsType result = returnTypeVar;
        for (var i = parameterTypeVars.Count - 1; i >= 0; i--)
            result = new FunctionType(parameterTypeVars[i], result);
        return result;
    }

    // Extracts the TypeVars assigned to each parameter from a curried FunctionType.
    private static List<GsType> ExtractParameterTypeVars(GsType functionType, int parameterCount)
    {
        var parameterTypes = new List<GsType>();
        var currentType    = functionType;

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

    // Extracts the return TypeVar from a curried FunctionType by traversing past all parameters.
    private static GsType ExtractReturnTypeVar(GsType functionType, int parameterCount)
    {
        var currentType = functionType;
        for (var i = 0; i < parameterCount; i++)
            if (currentType is FunctionType ft)
                currentType = ft.ReturnType;
        return currentType;
    }

    // Pre-populates the global environment with built-in names as fresh TypeVars.
    // Their concrete types are resolved at each call site by InferBuiltInCall.
    private TypeEnvironment CreateGlobalEnvironment()
    {
        var globalEnvironment = new TypeEnvironment();

        foreach (var name in PrecompiledCatalog.Functions.Keys)
            globalEnvironment.Register(name, FreshTypeVar());

        return globalEnvironment;
    }
}
