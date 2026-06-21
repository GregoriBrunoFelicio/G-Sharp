namespace GSharp.TypeChecker;

/// <summary>
///     Maps variable and function names to their types within a given scope.
///     Environments are chained — a child scope inherits all names from its parent
///     but can introduce new bindings that are invisible outside the child.
///     This models lexical scoping: function parameters exist only inside the function body,
///     `->` bindings exist only after their declaration, etc.
///     Example:
///     global scope: { add → FunctionType, println → UnitType }
///     function scope: { a → ?0, b → ?1 }   ← created by CreateChildScope()
/// </summary>
public class TypeEnvironment(TypeEnvironment? parentScope = null)
{
    private readonly Dictionary<string, GsType> _typesByName = new();

    /// <summary>Registers a name with its type in the current scope.</summary>
    public void Register(string name, GsType type)
    {
        _typesByName[name] = type;
    }

    /// <summary>
    ///     Looks up a name, walking up to parent scopes if not found locally.
    ///     Throws if the name is not bound anywhere in the scope chain.
    /// </summary>
    public GsType Lookup(string name)
    {
        if (_typesByName.TryGetValue(name, out var type))
            return type;

        if (parentScope is not null)
            return parentScope.Lookup(name);

        throw new Exception($"unbound name '{name}'");
    }

    /// <summary>
    ///     Tries to look up a name without throwing.
    ///     Returns false if the name is not bound anywhere in the scope chain.
    /// </summary>
    public bool TryLookup(string name, out GsType type)
    {
        if (_typesByName.TryGetValue(name, out type!))
            return true;

        if (parentScope is not null)
            return parentScope.TryLookup(name, out type);

        type = null!;
        return false;
    }

    /// <summary>
    ///     Creates a new child scope that inherits all bindings from this scope.
    ///     Used when entering a function body, if branch, or for loop body.
    /// </summary>
    public TypeEnvironment CreateChildScope()
    {
        return new TypeEnvironment(this);
    }
}