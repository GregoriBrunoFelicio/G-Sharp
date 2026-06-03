namespace GSharp.TypeChecker;

/// <summary>
/// Base type for all G# types.
/// Every expression in G# has a GsType assigned during type inference.
/// </summary>
public abstract record GsType;

/// <summary>Represents a 32-bit integer literal (e.g. 42).</summary>
public record IntType : GsType;

/// <summary>Represents a single-precision float literal (e.g. 2.5f).</summary>
public record FloatType : GsType;

/// <summary>Represents a double-precision float literal (e.g. 3.14d).</summary>
public record DoubleType : GsType;

/// <summary>Represents a decimal literal (e.g. 9.99m).</summary>
public record DecimalType : GsType;

/// <summary>Represents a string literal (e.g. "hello").</summary>
public record StringType : GsType;

/// <summary>Represents a boolean literal (true or false).</summary>
public record BoolType : GsType;

/// <summary>
/// Represents the absence of a meaningful return value.
/// Assigned to expressions that produce side effects only — println, while, etc.
/// Every expression must have a type in a purely functional language,
/// so Unit fills the role that void plays in imperative languages.
/// </summary>
public record UnitType : GsType;

/// <summary>
/// Represents a homogeneous array whose elements all have the same type.
/// Example: [1 2 3] has type ArrayType(IntType).
/// </summary>
public record ArrayType(GsType ElementType) : GsType;

/// <summary>
/// Represents a single-argument function type.
/// G# functions are curried — a two-argument function add a b is represented as:
/// FunctionType(IntType, FunctionType(IntType, IntType))
/// meaning: takes an Int, returns a function that takes an Int and returns an Int.
/// </summary>
public record FunctionType(GsType ParameterType, GsType ReturnType) : GsType;

/// <summary>
/// A type variable — a placeholder used during inference when the concrete type is not yet known.
/// The TypeInferrer creates fresh TypeVars (e.g. ?0, ?1, ?2) and the Unifier resolves them
/// to concrete types by solving the accumulated constraints.
/// </summary>
public record TypeVar(string Id) : GsType;
