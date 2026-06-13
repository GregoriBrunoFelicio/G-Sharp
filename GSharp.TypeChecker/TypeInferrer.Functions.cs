using GSharp.AST;

namespace GSharp.TypeChecker;

public partial class TypeInferrer
{
    // -------------------------------------------------------------------------
    // Function inference
    // -------------------------------------------------------------------------

    private void RegisterFunctionSignature(FunctionDeclaration fn, TypeEnvironment environment)
    {
        var parameterTypeVars = fn.Parameters.Select(_ => FreshTypeVar()).ToList();
        var returnTypeVar = FreshTypeVar();
        var functionType = BuildCurriedFunctionType(parameterTypeVars, returnTypeVar);
        environment.Register(fn.Name, functionType);
    }

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

    private GsType InferModuleCall(ModuleCallExpression moduleCall, TypeEnvironment environment)
    {
        var qualifiedName = $"{moduleCall.Module}.{moduleCall.Function}";

        if (BuiltinTypeRules.ContainsKey(qualifiedName))
            return InferBuiltinCall(qualifiedName, moduleCall.Arguments, environment);

        if (!environment.TryLookup(qualifiedName, out var calleeType))
            calleeType = FreshTypeVar();

        return ApplyArguments(calleeType, moduleCall.Arguments, environment);
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
    // Function type helpers
    // -------------------------------------------------------------------------

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
            if (currentType is not FunctionType ft)
                throw new Exception($"expected function type but got '{currentType}'");

            parameterTypes.Add(ft.ParameterType);
            currentType = ft.ReturnType;
        }

        return parameterTypes;
    }

    private static GsType ExtractReturnTypeVar(GsType functionType, int parameterCount)
    {
        var currentType = functionType;

        for (var i = 0; i < parameterCount; i++)
        {
            if (currentType is not FunctionType ft)
                throw new Exception($"expected function type but got '{currentType}'");

            currentType = ft.ReturnType;
        }

        return currentType;
    }
}