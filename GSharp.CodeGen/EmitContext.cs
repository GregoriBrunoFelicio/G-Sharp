using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
using GSharp.TypeChecker;

namespace GSharp.CodeGen;

public class EmitContext(
    Dictionary<string, MethodBuilder> functions,
    Dictionary<string, MethodBuilder> functionAdapters,
    Dictionary<Expression, GsType>? typeMap = null)
{
    public readonly Dictionary<string, LocalBuilder> Locals = new();
    public readonly Dictionary<string, int> Parameters = new();
    public readonly Dictionary<string, MethodBuilder> Functions = functions;
    public readonly Dictionary<string, MethodBuilder> FunctionAdapters = functionAdapters;
    public readonly Dictionary<string, MethodInfo> Builtins = new();
    public readonly Dictionary<Expression, GsType> TypeMap =
        typeMap ?? new Dictionary<Expression, GsType>(ReferenceEqualityComparer.Instance);
}
