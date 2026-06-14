using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
using GSharp.TypeChecker;

namespace GSharp.CodeGen;

public record TailCallInfo(string FunctionName, int ParameterCount, Label StartLabel);

public class EmitContext(
    Dictionary<string, MethodBuilder> functions,
    Dictionary<string, MethodBuilder> functionAdapters,
    Dictionary<Expression, GsType>? typeMap = null,
    Dictionary<string, Type[]>? functionParamTypes = null)
{
    public readonly Dictionary<string, LocalBuilder> Locals = new();
    public readonly Dictionary<string, (int Index, Type ClrType)> Parameters = new();
    public readonly Dictionary<string, MethodBuilder> Functions = functions;
    public readonly Dictionary<string, MethodBuilder> FunctionAdapters = functionAdapters;
    public readonly Dictionary<string, MethodInfo> Builtins = new();
    // Shared across parent and all child contexts so call sites and TCO can resolve typed param slots.
    public readonly Dictionary<string, Type[]> FunctionParamTypes = functionParamTypes ?? new();
    public readonly Dictionary<Expression, GsType> TypeMap =
        typeMap ?? new Dictionary<Expression, GsType>(ReferenceEqualityComparer.Instance);

    public TailCallInfo? TailCall { get; set; }
}
