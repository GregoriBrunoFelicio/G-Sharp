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
        ["array.contains"] = new BuiltinTypeRule((_, _) => new BoolType(),
            [(arrayType, _) => arrayType, (arrayType, _) => arrayType.ElementType]),
        ["array.indexOf"] = new BuiltinTypeRule((_, _) => new IntType(),
            [(arrayType, _) => arrayType, (arrayType, _) => arrayType.ElementType]),
        ["array.slice"] = new BuiltinTypeRule((arrayType, _) => arrayType,
            [(arrayType, _) => arrayType, (_, _) => new IntType(), (_, _) => new IntType()]),
        ["array.range"] = new BuiltinTypeRule((_, _) => new ArrayType(new IntType()),
            [(_, _) => new IntType(), (_, _) => new IntType()]),
        ["array.max"] = new BuiltinTypeRule((arrayType, _) => arrayType.ElementType, [(arrayType, _) => arrayType]),
        ["array.min"] = new BuiltinTypeRule((arrayType, _) => arrayType.ElementType, [(arrayType, _) => arrayType]),
        ["string.from"] = new BuiltinTypeRule((_, _) => new StringType(), [null]),
        ["string.len"] = new BuiltinTypeRule((_, _) => new IntType(), [(_, _) => new StringType()]),
        ["string.upper"] = new BuiltinTypeRule((_, _) => new StringType(), [(_, _) => new StringType()]),
        ["string.lower"] = new BuiltinTypeRule((_, _) => new StringType(), [(_, _) => new StringType()]),
        ["string.trim"] = new BuiltinTypeRule((_, _) => new StringType(), [(_, _) => new StringType()]),
        ["string.contains"] = new BuiltinTypeRule((_, _) => new BoolType(),
            [(_, _) => new StringType(), (_, _) => new StringType()]),
        ["string.startsWith"] = new BuiltinTypeRule((_, _) => new BoolType(),
            [(_, _) => new StringType(), (_, _) => new StringType()]),
        ["string.endsWith"] = new BuiltinTypeRule((_, _) => new BoolType(),
            [(_, _) => new StringType(), (_, _) => new StringType()]),
        ["string.split"] = new BuiltinTypeRule((_, _) => new ArrayType(new StringType()),
            [(_, _) => new StringType(), (_, _) => new StringType()]),
        ["string.join"] = new BuiltinTypeRule((_, _) => new StringType(),
            [(_, _) => new ArrayType(new StringType()), (_, _) => new StringType()]),
        ["string.replace"] = new BuiltinTypeRule((_, _) => new StringType(),
            [(_, _) => new StringType(), (_, _) => new StringType(), (_, _) => new StringType()]),
        ["string.slice"] = new BuiltinTypeRule((_, _) => new StringType(),
            [(_, _) => new StringType(), (_, _) => new IntType(), (_, _) => new IntType()]),
        ["string.toInt"] = new BuiltinTypeRule((_, _) => new IntType(), [(_, _) => new StringType()]),
        ["string.toFloat"] = new BuiltinTypeRule((_, _) => new FloatType(), [(_, _) => new StringType()]),

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
            ]),

        // any/all: (arr: [elem], fn: elem -> bool) -> bool
        ["array.any"] = new BuiltinTypeRule(
            (_, _) => new BoolType(),
            [
                (arrayType, _) => arrayType,
                (arrayType, _) => new FunctionType(arrayType.ElementType, new BoolType())
            ]),
        ["array.all"] = new BuiltinTypeRule(
            (_, _) => new BoolType(),
            [
                (arrayType, _) => arrayType,
                (arrayType, _) => new FunctionType(arrayType.ElementType, new BoolType())
            ]),

        // abs/floor/ceil/round preserve the operand's numeric type: argument and return type
        // share one fresh TypeVar (resultTypeVar), same trick as fold's accumulator above.
        ["math.abs"] = new BuiltinTypeRule((_, resultTypeVar) => resultTypeVar,
            [(_, resultTypeVar) => resultTypeVar]),
        ["math.floor"] = new BuiltinTypeRule((_, resultTypeVar) => resultTypeVar,
            [(_, resultTypeVar) => resultTypeVar]),
        ["math.ceil"] = new BuiltinTypeRule((_, resultTypeVar) => resultTypeVar,
            [(_, resultTypeVar) => resultTypeVar]),
        ["math.round"] = new BuiltinTypeRule((_, resultTypeVar) => resultTypeVar,
            [(_, resultTypeVar) => resultTypeVar]),
        // sqrt/pow: result can't stay in the operand's type (an irrational result), so they
        // always return double and accept any numeric type unconstrained (null = no constraint).
        ["math.sqrt"] = new BuiltinTypeRule((_, _) => new DoubleType(), [null]),
        ["math.pow"] = new BuiltinTypeRule((_, _) => new DoubleType(), [null, null]),
        // min/max: both operands and the result share one type, like a binary operator.
        ["math.min"] = new BuiltinTypeRule((_, resultTypeVar) => resultTypeVar,
            [(_, resultTypeVar) => resultTypeVar, (_, resultTypeVar) => resultTypeVar]),
        ["math.max"] = new BuiltinTypeRule((_, resultTypeVar) => resultTypeVar,
            [(_, resultTypeVar) => resultTypeVar, (_, resultTypeVar) => resultTypeVar]),
        ["math.mod"] = new BuiltinTypeRule((_, resultTypeVar) => resultTypeVar,
            [(_, resultTypeVar) => resultTypeVar, (_, resultTypeVar) => resultTypeVar]),
        ["math.pi"] = new BuiltinTypeRule((_, _) => new DoubleType(), []),
        ["math.e"] = new BuiltinTypeRule((_, _) => new DoubleType(), [])
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