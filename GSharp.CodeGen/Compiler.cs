using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;
using GSharp.Stdlib;
using GSharp.TypeChecker;
using Type = System.Type;

namespace GSharp.CodeGen;

public class Compiler
{
    private static readonly ConstructorInfo FuncObjectArrayObjectCtor =
        typeof(Func<object[], object>).GetConstructors()[0];
    private static readonly ConstructorInfo FuncObjectObjectCtor =
        typeof(Func<object, object>).GetConstructors()[0];
    private static readonly ConstructorInfo GsFunctionArityNCtor =
        typeof(GSharpFunction).GetConstructor([typeof(Func<object[], object>)])!;
    private static readonly ConstructorInfo GsFunctionArity1Ctor =
        typeof(GSharpFunction).GetConstructor([typeof(Func<object, object>)])!;

    private static void RegisterBuiltins(EmitContext context)
    {
        ArrayBuiltins.Register(context.Builtins);
        StringBuiltins.Register(context.Builtins);
    }

    private static (MethodBuilder, TypeBuilder) CreateBuilders()
    {
        var assemblyName = new AssemblyName("GSharpRuntimeAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        var typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Public);
        var methodBuilder = typeBuilder.DefineMethod(
            "Main",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(void),
            Type.EmptyTypes);
        return (methodBuilder, typeBuilder);
    }

    // Emits a static type initializer (<clinit>) that creates one GSharpFunction
    // instance per declared function and stores it in a static field. This means
    // referencing a function as a first-class value (e.g. passing `double` to
    // `map`) loads a field instead of allocating two objects every time.
    private static void EmitStaticInitializer(
        TypeBuilder typeBuilder,
        Dictionary<string, MethodBuilder> adapters,
        Dictionary<string, MethodBuilder> adapters1,
        Dictionary<string, FieldBuilder> functionFields)
    {
        if (functionFields.Count == 0) return;

        var cctor   = typeBuilder.DefineTypeInitializer();
        var cctorIl = cctor.GetILGenerator();

        foreach (var (name, field) in functionFields)
        {
            var isArity1 = adapters1.ContainsKey(name);
            cctorIl.Emit(OpCodes.Ldnull);
            cctorIl.Emit(OpCodes.Ldftn, isArity1 ? adapters1[name] : adapters[name]);
            cctorIl.Emit(OpCodes.Newobj, isArity1 ? FuncObjectObjectCtor      : FuncObjectArrayObjectCtor);
            cctorIl.Emit(OpCodes.Newobj, isArity1 ? GsFunctionArity1Ctor      : GsFunctionArityNCtor);
            cctorIl.Emit(OpCodes.Stsfld, field);
        }

        cctorIl.Emit(OpCodes.Ret);
    }

    public void CompileAndRun(
        List<Expression> expressions,
        Dictionary<string, List<Expression>>? modules = null,
        Dictionary<Expression, GsType>? typeMap = null)
    {
        try
        {
            var (methodBuilder, typeBuilder) = CreateBuilders();

            var functions       = new Dictionary<string, MethodBuilder>();
            var adapters        = new Dictionary<string, MethodBuilder>();
            var adapters1       = new Dictionary<string, MethodBuilder>();
            var functionFields  = new Dictionary<string, FieldBuilder>();
            var functionParamTypes = new Dictionary<string, Type[]>();

            foreach (var (moduleName, moduleExprs) in modules ?? [])
                foreach (var fn in moduleExprs.OfType<FunctionDeclaration>())
                    FunctionEmitter.Define(typeBuilder, fn, functions, adapters, typeMap,
                        prefix: moduleName + ".", functionParamTypes: functionParamTypes,
                        adapters1: adapters1, functionFields: functionFields);

            foreach (var fn in expressions.OfType<FunctionDeclaration>())
                FunctionEmitter.Define(typeBuilder, fn, functions, adapters, typeMap,
                    functionParamTypes: functionParamTypes,
                    adapters1: adapters1, functionFields: functionFields);

            EmitStaticInitializer(typeBuilder, adapters, adapters1, functionFields);

            var context = new EmitContext(functions, adapters, typeMap, functionParamTypes,
                functionAdapters1: adapters1, functionFields: functionFields);
            RegisterBuiltins(context);

            foreach (var (moduleName, moduleExprs) in modules ?? [])
                foreach (var fn in moduleExprs.OfType<FunctionDeclaration>())
                    FunctionEmitter.Emit(fn, context, prefix: moduleName + ".");

            foreach (var fn in expressions.OfType<FunctionDeclaration>())
                FunctionEmitter.Emit(fn, context);

            var il = methodBuilder.GetILGenerator();

            foreach (var expression in expressions.Where(e =>
                         e is not FunctionDeclaration and not ImportDeclaration))
            {
                ExpressionEmitter.EmitToStack(il, expression, context);
                il.Emit(OpCodes.Pop);
            }

            if (context.Functions.TryGetValue("main", out var userMain))
            {
                il.Emit(OpCodes.Call, userMain);
                il.Emit(OpCodes.Pop);
            }

            il.Emit(OpCodes.Ret);

            var programType = typeBuilder.CreateType();

            var main = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static)
                ?? throw new Exception("Method 'Main' was not found.");

            main.Invoke(null, null);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.InnerException?.Message ?? exception.Message);
            throw;
        }
    }
}
