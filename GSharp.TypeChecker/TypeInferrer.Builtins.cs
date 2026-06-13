using GSharp.AST;

namespace GSharp.TypeChecker;

public partial class TypeInferrer
{
    private static readonly Dictionary<string, BuiltinTypeRule> BuiltinTypeRules = new()
    {
        ["array.head"] = new BuiltinTypeRule(arrayType => arrayType.ElementType, [arrayType => arrayType]),
        ["array.last"] = new BuiltinTypeRule(arrayType => arrayType.ElementType, [arrayType => arrayType]),
        ["array.tail"] = new BuiltinTypeRule(arrayType => arrayType, [arrayType => arrayType]),
        ["array.reverse"] = new BuiltinTypeRule(arrayType => arrayType, [arrayType => arrayType]),
        ["array.sort"] = new BuiltinTypeRule(arrayType => arrayType, [arrayType => arrayType]),
        ["array.len"] = new BuiltinTypeRule(_ => new IntType(), [arrayType => arrayType]),
        ["array.empty"] = new BuiltinTypeRule(_ => new BoolType(), [arrayType => arrayType]),
        ["array.concat"] =
            new BuiltinTypeRule(arrayType => arrayType, [arrayType => arrayType, arrayType => arrayType]),
        ["array.take"] = new BuiltinTypeRule(arrayType => arrayType, [arrayType => arrayType, _ => new IntType()]),
        ["string.from"] = new BuiltinTypeRule(_ => new StringType(), [null])
    };

    // -------------------------------------------------------------------------
    // Builtin inference
    // -------------------------------------------------------------------------

    private GsType InferBuiltinCall(string name, List<Expression> expressions, TypeEnvironment environment)
    {
        var rule = BuiltinTypeRules[name];

        if (expressions.Count != rule.ArgumentConstraints.Count)
        {
            var argWord = rule.ArgumentConstraints.Count == 1 ? "argument" : "arguments";
            throw new Exception(
                $"'{name}' expects {rule.ArgumentConstraints.Count} {argWord} but got {expressions.Count}");
        }

        var elementTypeVar = FreshTypeVar();
        var arrayType = new ArrayType(elementTypeVar);

        for (var i = 0; i < expressions.Count; i++)
        {
            var expressionType = InferExpression(expressions[i], environment);
            var expectedType = rule.ArgumentConstraints[i]?.Invoke(arrayType);
            if (expectedType is not null)
                _constraints.Add(new TypeConstraint(expressionType, expectedType));
        }

        return rule.ReturnType(arrayType);
    }
    // -------------------------------------------------------------------------
    // Builtin type rules
    // -------------------------------------------------------------------------

    // Each entry describes the type signature of a builtin:
    // - ReturnType: given a fresh arrayType, what does the function return
    // - ArgumentConstraints: per argument, what type it must be (null = any type, no constraint)
    private record BuiltinTypeRule(
        Func<ArrayType, GsType> ReturnType,
        IReadOnlyList<Func<ArrayType, GsType>?> ArgumentConstraints
    );
}