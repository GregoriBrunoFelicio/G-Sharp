namespace GSharp.TypeChecker;

/// <summary>
/// Solves a list of type constraints using Robinson's unification algorithm.
///
/// Takes all the equality constraints collected by the TypeInferrer
/// and finds a Substitution (a mapping TypeVar → GsType) that satisfies all of them.
/// If no such substitution exists — e.g. IntType = StringType — throws a type error.
///
/// Uses a queue to process constraints iteratively (no recursion).
/// When a composite constraint like FunctionType = FunctionType is encountered,
/// it is broken into smaller constraints and re-added to the queue.
/// </summary>
public static class Unifier
{
    public static Substitution Unify(List<TypeConstraint> constraints)
    {
        var substitution    = new Substitution();
        var constraintQueue = new Queue<TypeConstraint>(constraints);

        while (constraintQueue.Count > 0)
        {
            var constraint = constraintQueue.Dequeue();

            // Apply current substitution before comparing — a TypeVar may already be resolved
            var leftType  = substitution.Apply(constraint.Left);
            var rightType = substitution.Apply(constraint.Right);

            // Already equal — nothing to do
            if (leftType == rightType) continue;

            // Normalize: TypeVar always on the left
            if (leftType is not TypeVar && rightType is TypeVar)
                (leftType, rightType) = (rightType, leftType);

            switch (leftType, rightType)
            {
                // One side is a TypeVar — bind it to the other type
                case (TypeVar tv, GsType resolved):
                    if (OccursIn(tv.Id, resolved))
                        throw new Exception($"{Position(constraint)}type error: infinite type — '{tv.Id}' occurs in '{resolved}'");
                    substitution.Bind(tv.Id, resolved);
                    break;

                // Both are function types — decompose into two smaller constraints,
                // carrying the source position forward so a deep mismatch still has a line.
                case (FunctionType leftFunction, FunctionType rightFunction):
                    constraintQueue.Enqueue(new TypeConstraint(leftFunction.ParameterType, rightFunction.ParameterType, constraint.Line, constraint.Column));
                    constraintQueue.Enqueue(new TypeConstraint(leftFunction.ReturnType,    rightFunction.ReturnType,    constraint.Line, constraint.Column));
                    break;

                // Both are array types — constrain their element types
                case (ArrayType leftArray, ArrayType rightArray):
                    constraintQueue.Enqueue(new TypeConstraint(leftArray.ElementType, rightArray.ElementType, constraint.Line, constraint.Column));
                    break;

                // Two different concrete types — impossible to unify
                default:
                    throw new Exception($"{Position(constraint)}type mismatch: expected '{leftType}', got '{rightType}'");
            }
        }

        return substitution;
    }

    // Prefixes the 1-based source line (matching the lexer/parser error format "N: ...")
    // when known, so DocumentAnalyzer can map the diagnostic onto the right line.
    private static string Position(TypeConstraint constraint) =>
        constraint.Line > 0 ? $"{constraint.Line}: " : "";

    /// <summary>
    /// Checks whether a TypeVar appears anywhere inside a type.
    /// Prevents binding a TypeVar to a type that contains itself (e.g. ?a = Array(?a)),
    /// which would create an infinitely recursive type and loop forever during Apply.
    /// </summary>
    private static bool OccursIn(string typeVarId, GsType type) => type switch
    {
        TypeVar tv      => tv.Id == typeVarId,
        FunctionType ft => OccursIn(typeVarId, ft.ParameterType) || OccursIn(typeVarId, ft.ReturnType),
        ArrayType at    => OccursIn(typeVarId, at.ElementType),
        _               => false
    };
}
