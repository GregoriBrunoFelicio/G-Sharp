using GSharp.Compiler.AST;

namespace GSharp.Compiler.TypeChecker;

public partial class TypeInferrer
{
    private static readonly Dictionary<string, BuiltinTypeRule> BuiltinTypeRules = new()
    {
        ["array.head"] = new BuiltinTypeRule((arrayType, _) => arrayType.ElementType, [(arrayType, _) => arrayType]),
        ["array.last"] = new BuiltinTypeRule((arrayType, _) => arrayType.ElementType, [(arrayType, _) => arrayType]),
        ["array.tail"] = new BuiltinTypeRule((arrayType, _) => arrayType, [(arrayType, _) => arrayType]),
        ["array.reverse"] = new BuiltinTypeRule((arrayType, _) => arrayType, [(arrayType, _) => arrayType]),
        ["array.sort"] = new BuiltinTypeRule((arrayType, _) => arrayType, [(arrayType, _) => arrayType]),
        ["array.len"] = new BuiltinTypeRule((_, _) => new IntType(), [(arrayType, _) => arrayType]),
        ["array.empty"] = new BuiltinTypeRule((_, _) => new BoolType(), [(arrayType, _) => arrayType]),
        ["array.concat"] =
            new BuiltinTypeRule((arrayType, _) => arrayType, [null, null]),
        ["array.take"] = new BuiltinTypeRule((arrayType, _) => arrayType,
            [(arrayType, _) => arrayType, (_, _) => new IntType()]),
        ["string.from"] = new BuiltinTypeRule((_, _) => new StringType(), [null]),

        // map: (arr: [elem], fn: elem -> result) -> [result] — result element type can differ
        // from the input element type, so it needs its own fresh type var (resultTypeVar).
        ["array.map"] = new BuiltinTypeRule(
            (_, resultTypeVar) => new ArrayType(resultTypeVar),
            [
                (arrayType, _) => arrayType,
                (arrayType, resultTypeVar) => new FunctionType(arrayType.ElementType, resultTypeVar)
            ]),

        // filter: (arr: [elem], fn: elem -> bool) -> [elem] — element type is preserved.
        ["array.filter"] = new BuiltinTypeRule(
            (arrayType, _) => arrayType,
            [
                (arrayType, _) => arrayType,
                (arrayType, _) => new FunctionType(arrayType.ElementType, new BoolType())
            ]),

        // fold: (arr: [elem], seed: acc, fn: acc -> elem -> acc) -> acc
        ["array.fold"] = new BuiltinTypeRule(
            (_, resultTypeVar) => resultTypeVar,
            [
                (arrayType, _) => arrayType,
                (_, resultTypeVar) => resultTypeVar,
                (arrayType, resultTypeVar) =>
                    new FunctionType(resultTypeVar, new FunctionType(arrayType.ElementType, resultTypeVar))
            ])
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
        var resultTypeVar = FreshTypeVar();

        for (var i = 0; i < expressions.Count; i++)
        {
            var expressionType = InferExpression(expressions[i], environment);
            var expectedType = rule.ArgumentConstraints[i]?.Invoke(arrayType, resultTypeVar);
            if (expectedType is not null)
                _constraints.Add(new TypeConstraint(expressionType, expectedType));
        }

        return rule.ReturnType(arrayType, resultTypeVar);
    }
    // -------------------------------------------------------------------------
    // Builtin type rules
    // -------------------------------------------------------------------------

    // Each entry describes the type signature of a builtin:
    // - ReturnType: given a fresh arrayType and a fresh resultTypeVar, what does the function return
    // - ArgumentConstraints: per argument, what type it must be (null = any type, no constraint).
    //   resultTypeVar is only meaningful for map/filter/fold (the callback's result/accumulator type);
    //   every other rule ignores it.
    private record BuiltinTypeRule(
        Func<ArrayType, TypeVar, GsType> ReturnType,
        IReadOnlyList<Func<ArrayType, TypeVar, GsType>?> ArgumentConstraints
    );
}