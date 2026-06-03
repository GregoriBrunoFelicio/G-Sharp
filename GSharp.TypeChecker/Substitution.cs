namespace GSharp.TypeChecker;

/// <summary>
/// The solution produced by the Unifier — a mapping from TypeVar ids to their resolved types.
///
/// After the Unifier solves all constraints, calling Apply() on any type replaces
/// every TypeVar with its resolved concrete type. TypeVars that remain unresolved
/// (not constrained by anything) stay as TypeVars — they represent truly polymorphic positions.
///
/// Example:
///   Constraints: ?0 = IntType, ?1 = FunctionType(?0, BoolType)
///   Apply(FunctionType(?0, ?1)) → FunctionType(IntType, FunctionType(IntType, BoolType))
/// </summary>
public class Substitution
{
    private readonly Dictionary<string, GsType> _resolvedTypesByVarId = new();

    /// <summary>Binds a TypeVar id to its resolved type.</summary>
    public void Bind(string typeVarId, GsType resolvedType) =>
        _resolvedTypesByVarId[typeVarId] = resolvedType;

    /// <summary>
    /// Recursively replaces all TypeVars in the given type with their resolved types.
    /// Follows chains: if ?0 → ?1 and ?1 → IntType, Apply(?0) returns IntType.
    /// </summary>
    public GsType Apply(GsType type) => type switch
    {
        TypeVar tv when _resolvedTypesByVarId.TryGetValue(tv.Id, out var resolved) => Apply(resolved),
        TypeVar tv                                                                   => tv,
        FunctionType ft => new FunctionType(Apply(ft.ParameterType), Apply(ft.ReturnType)),
        ArrayType at    => new ArrayType(Apply(at.ElementType)),
        _               => type
    };
}
