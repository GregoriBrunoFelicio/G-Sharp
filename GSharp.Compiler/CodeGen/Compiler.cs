using System.Reflection;
using System.Reflection.Emit;
using GSharp.Compiler.AST;
using GSharp.Compiler.CodeGen.Helpers;
using GSharp.Compiler.Stdlib;
using GSharp.Compiler.TypeChecker;
using Type = System.Type;

namespace GSharp.Compiler.CodeGen;

public class Compiler
{
    private static readonly ConstructorInfo FuncObjectArrayObjectCtor =
        typeof(Func<object[], object>).GetConstructors()[0];

    private static readonly ConstructorInfo FuncObjectObjectCtor =
        typeof(Func<object, object>).GetConstructors()[0];

    private static readonly ConstructorInfo GsFunctionArityNCtor =
        typeof(GSharpFunction).GetConstructor([typeof(Func<object[], object>), typeof(int)])!;

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
        Dictionary<string, FieldBuilder> functionFields,
        Dictionary<string, Type[]> functionParamTypes)
    {
        if (functionFields.Count == 0) return;

        var cctor = typeBuilder.DefineTypeInitializer();
        var cctorIl = cctor.GetILGenerator();

        foreach (var (name, field) in functionFields)
        {
            var isArity1 = adapters1.ContainsKey(name);
            cctorIl.Emit(OpCodes.Ldnull);
            cctorIl.Emit(OpCodes.Ldftn, isArity1 ? adapters1[name] : adapters[name]);
            cctorIl.Emit(OpCodes.Newobj, isArity1 ? FuncObjectObjectCtor : FuncObjectArrayObjectCtor);
            if (isArity1)
            {
                cctorIl.Emit(OpCodes.Newobj, GsFunctionArity1Ctor);
            }
            else
            {
                cctorIl.Emit(OpCodes.Ldc_I4, functionParamTypes[name].Length);
                cctorIl.Emit(OpCodes.Newobj, GsFunctionArityNCtor);
            }
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

            var functions = new Dictionary<string, MethodBuilder>();
            var adapters = new Dictionary<string, MethodBuilder>();
            var adapters1 = new Dictionary<string, MethodBuilder>();
            var functionFields = new Dictionary<string, FieldBuilder>();
            var functionParamTypes = new Dictionary<string, Type[]>();

            var lambdaFunctionNames = new Dictionary<Expression, string>(ReferenceEqualityComparer.Instance);
            var topLevelLift = LambdaLifter.Lift(expressions);
            foreach (var (lambda, name) in topLevelLift.LambdaNames)
                lambdaFunctionNames[lambda] = name;

            var moduleLifts = new Dictionary<string, LambdaLiftResult>();
            foreach (var (moduleName, moduleExprs) in modules ?? [])
            {
                var lift = LambdaLifter.Lift(moduleExprs, moduleName + ".");
                moduleLifts[moduleName] = lift;
                foreach (var (lambda, name) in lift.LambdaNames)
                    lambdaFunctionNames[lambda] = name;
            }

            foreach (var (moduleName, moduleExprs) in modules ?? [])
            {
                foreach (var fn in moduleExprs.OfType<FunctionDeclaration>())
                    ExpressionEmitter.DefineFunction(typeBuilder, fn, functions, adapters, typeMap,
                        moduleName + ".", functionParamTypes,
                        adapters1, functionFields);

                foreach (var fn in moduleLifts[moduleName].LiftedFunctions)
                    ExpressionEmitter.DefineFunction(typeBuilder, fn, functions, adapters, typeMap,
                        moduleName + ".", functionParamTypes,
                        adapters1, functionFields);
            }

            foreach (var fn in expressions.OfType<FunctionDeclaration>())
                ExpressionEmitter.DefineFunction(typeBuilder, fn, functions, adapters, typeMap,
                    functionParamTypes: functionParamTypes,
                    adapters1: adapters1, functionFields: functionFields);

            foreach (var fn in topLevelLift.LiftedFunctions)
                ExpressionEmitter.DefineFunction(typeBuilder, fn, functions, adapters, typeMap,
                    functionParamTypes: functionParamTypes,
                    adapters1: adapters1, functionFields: functionFields);

            EmitStaticInitializer(typeBuilder, adapters, adapters1, functionFields, functionParamTypes);

            var context = new EmitContext(functions, adapters, typeMap, functionParamTypes,
                adapters1, functionFields, lambdaFunctionNames);
            RegisterBuiltins(context);

            foreach (var (moduleName, moduleExprs) in modules ?? [])
            {
                foreach (var fn in moduleExprs.OfType<FunctionDeclaration>())
                    ExpressionEmitter.EmitFunction(fn, context, moduleName + ".");

                foreach (var fn in moduleLifts[moduleName].LiftedFunctions)
                    ExpressionEmitter.EmitFunction(fn, context, moduleName + ".");
            }

            foreach (var fn in expressions.OfType<FunctionDeclaration>())
                ExpressionEmitter.EmitFunction(fn, context);

            foreach (var fn in topLevelLift.LiftedFunctions)
                ExpressionEmitter.EmitFunction(fn, context);

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