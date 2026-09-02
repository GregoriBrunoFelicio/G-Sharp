using System.Reflection;
using System.Reflection.Emit;
using GSharp.Compiler.AST;
using GSharp.Compiler.TypeChecker;

namespace GSharp.Compiler.CodeGen;

public record TailCallInfo(string FunctionName, int ParameterCount, Label StartLabel);

public class EmitContext(
    Dictionary<string, MethodBuilder> functions,
    Dictionary<string, MethodBuilder> functionAdapters,
    Dictionary<Expression, GsType>? typeMap = null,
    Dictionary<string, Type[]>? functionParamTypes = null,
    Dictionary<string, MethodBuilder>? functionAdapters1 = null,
    Dictionary<string, FieldBuilder>? functionFields = null,
    Dictionary<Expression, string>? lambdaFunctionNames = null)
{
    public readonly Dictionary<string, MethodInfo> Builtins = new();
    public readonly Dictionary<string, MethodBuilder> FunctionAdapters = functionAdapters;

    public readonly Dictionary<Expression, string> LambdaFunctionNames =
        lambdaFunctionNames ?? new Dictionary<Expression, string>(ReferenceEqualityComparer.Instance);

    public readonly Dictionary<string, MethodBuilder> FunctionAdapters1 =
        functionAdapters1 ?? new Dictionary<string, MethodBuilder>();

    public readonly Dictionary<string, FieldBuilder> FunctionFields =
        functionFields ?? new Dictionary<string, FieldBuilder>();

    public readonly Dictionary<string, Type[]> FunctionParamTypes =
        functionParamTypes ?? new Dictionary<string, Type[]>();

    public readonly Dictionary<string, MethodBuilder> Functions = functions;
    public readonly Dictionary<string, LocalBuilder> Locals = new();
    public readonly Dictionary<string, (int Index, Type ClrType)> Parameters = new();

    public readonly Dictionary<Expression, GsType> TypeMap =
        typeMap ?? new Dictionary<Expression, GsType>(ReferenceEqualityComparer.Instance);

    public TailCallInfo? TailCall { get; set; }
}