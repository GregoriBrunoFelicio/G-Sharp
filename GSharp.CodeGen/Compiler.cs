using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
using GSharp.Stdlib;
using GSharp.TypeChecker;
using Type = System.Type;

namespace GSharp.CodeGen;

public class Compiler
{
    private static void RegisterBuiltins(EmitContext ctx)
    {
        ArrayBuiltins.Register(ctx.Builtins);
        StringBuiltins.Register(ctx.Builtins);
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

            foreach (var (moduleName, moduleExprs) in modules ?? [])
                foreach (var fn in moduleExprs.OfType<FunctionDeclaration>())
                    FunctionEmitter.Define(typeBuilder, fn, functions, adapters, prefix: moduleName + ".");

            foreach (var fn in expressions.OfType<FunctionDeclaration>())
                FunctionEmitter.Define(typeBuilder, fn, functions, adapters);

            var ctx = new EmitContext(functions, adapters, typeMap);
            RegisterBuiltins(ctx);

            foreach (var (moduleName, moduleExprs) in modules ?? [])
                foreach (var fn in moduleExprs.OfType<FunctionDeclaration>())
                    FunctionEmitter.Emit(fn, ctx, prefix: moduleName + ".");

            foreach (var fn in expressions.OfType<FunctionDeclaration>())
                FunctionEmitter.Emit(fn, ctx);

            var il = methodBuilder.GetILGenerator();

            foreach (var expr in expressions.Where(e =>
                         e is not FunctionDeclaration and not ImportDeclaration))
            {
                ExpressionEmitter.EmitToStack(il, expr, ctx);
                il.Emit(OpCodes.Pop);
            }

            if (ctx.Functions.TryGetValue("main", out var userMain))
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
