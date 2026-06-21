using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
using GSharp.TypeChecker;

namespace GSharp.CodeGen;

public static class FunctionEmitter
{

    public static void Define(
        TypeBuilder typeBuilder,
        FunctionDeclaration fn,
        Dictionary<string, MethodBuilder> functions,
        Dictionary<string, MethodBuilder> adapters,
        Dictionary<Expression, GsType>? typeMap = null,
        string prefix = "",
        Dictionary<string, Type[]>? functionParamTypes = null)
    {
        var qualifiedName = prefix + fn.Name;
        var paramTypes = ResolveParameterClrTypes(fn, typeMap);

        var method = typeBuilder.DefineMethod(
            qualifiedName,
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(object),
            paramTypes);

        functions[qualifiedName] = method;
        functionParamTypes?[qualifiedName] = paramTypes;

        var adapter = typeBuilder.DefineMethod(
            qualifiedName + "__adapter",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(object),
            [typeof(object[])]);

        adapters[qualifiedName] = adapter;
    }


    public static void Emit(FunctionDeclaration fn, EmitContext context, string prefix = "")
    {
        EmitMainBody(fn, context, prefix);
        EmitAdapter(fn, context, prefix);
    }

    private static void EmitMainBody(FunctionDeclaration fn, EmitContext context, string prefix)
    {
        var qualifiedName = prefix + fn.Name;
        var method = context.Functions[qualifiedName];
        var il = method.GetILGenerator();

        var functionContext = new EmitContext(context.Functions, context.FunctionAdapters, context.TypeMap, context.FunctionParamTypes);
        foreach (var (builtinName, builtinMethod) in context.Builtins)
            functionContext.Builtins[builtinName] = builtinMethod;

        var paramClrTypes = ResolveParameterClrTypes(fn, context.TypeMap);
        for (var i = 0; i < fn.Parameters.Count; i++)
            functionContext.Parameters[fn.Parameters[i]] = (i, paramClrTypes[i]);

        var startLabel = il.DefineLabel();
        il.MarkLabel(startLabel);
        functionContext.TailCall = new TailCallInfo(qualifiedName, fn.Parameters.Count, startLabel);

        var body = fn.Body;

        for (var i = 0; i < body.Count - 1; i++)
        {
            ExpressionEmitter.EmitToStack(il, body[i], functionContext);
            il.Emit(OpCodes.Pop);
        }

        if (body.Count > 0)
            TailCallEmitter.EmitTail(il, body[^1], functionContext);
        else
            il.Emit(OpCodes.Ldnull);

        il.Emit(OpCodes.Ret);
    }

    private static void EmitAdapter(FunctionDeclaration fn, EmitContext context, string prefix)
    {
        var qualifiedName = prefix + fn.Name;
        var adapter = context.FunctionAdapters[qualifiedName];
        var il = adapter.GetILGenerator();

        var paramClrTypes = ResolveParameterClrTypes(fn, context.TypeMap);
        for (var i = 0; i < fn.Parameters.Count; i++)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, i);
            il.Emit(OpCodes.Ldelem_Ref);
            if (paramClrTypes[i].IsValueType)
                il.Emit(OpCodes.Unbox_Any, paramClrTypes[i]);
        }

        il.Emit(OpCodes.Call, context.Functions[qualifiedName]);
        il.Emit(OpCodes.Ret);
    }

    private static Type[] ResolveParameterClrTypes(FunctionDeclaration fn, Dictionary<Expression, GsType>? typeMap)
    {
        if (typeMap is null || !typeMap.TryGetValue(fn, out var fnType))
            return Enumerable.Repeat(typeof(object), fn.Parameters.Count).ToArray();

        var clrTypes = new Type[fn.Parameters.Count];
        var currentType = fnType;
        for (var i = 0; i < fn.Parameters.Count; i++)
        {
            clrTypes[i] = currentType is FunctionType ft ? GsTypeToClr(ft.ParameterType) : typeof(object);
            currentType = currentType is FunctionType ft2 ? ft2.ReturnType : currentType;
        }
        return clrTypes;
    }

    private static Type GsTypeToClr(GsType type) => type switch
    {
        IntType     => typeof(int),
        FloatType   => typeof(float),
        DoubleType  => typeof(double),
        DecimalType => typeof(decimal),
        BoolType    => typeof(bool),
        _           => typeof(object)
    };
}
