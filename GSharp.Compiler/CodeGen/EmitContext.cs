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
    Dictionary<string, Type[]>? functionParamTypes = null,
    Dictionary<string, MethodBuilder>? functionAdapters1 = null,
    Dictionary<string, FieldBuilder>? functionFields = null)
{
    public readonly Dictionary<string, LocalBuilder>              Locals          = new();
    public readonly Dictionary<string, (int Index, Type ClrType)> Parameters     = new();
    public readonly Dictionary<string, MethodBuilder>             Functions       = functions;
    public readonly Dictionary<string, MethodBuilder>             FunctionAdapters  = functionAdapters;
    public readonly Dictionary<string, MethodBuilder>             FunctionAdapters1 = functionAdapters1 ?? new();
    public readonly Dictionary<string, FieldBuilder>              FunctionFields    = functionFields    ?? new();
    public readonly Dictionary<string, MethodInfo>                Builtins          = new();
    public readonly Dictionary<string, Type[]>                    FunctionParamTypes =
        functionParamTypes ?? new Dictionary<string, Type[]>();
    public readonly Dictionary<Expression, GsType> TypeMap =
        typeMap ?? new Dictionary<Expression, GsType>(ReferenceEqualityComparer.Instance);

    public TailCallInfo? TailCall { get; set; }
}
