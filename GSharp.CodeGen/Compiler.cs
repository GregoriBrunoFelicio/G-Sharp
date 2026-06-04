using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
using GSharp.TypeChecker;
using Type = System.Type;

namespace GSharp.CodeGen;

// Entry point for the G# compiler.
//
// Receives a fully-parsed AST (list of expressions) and compiles it to .NET IL
// using System.Reflection.Emit. The compiled code runs in-memory.
//
// Compilation happens in three passes:
//
//   Pass 1 — Function registration:
//     Define a MethodBuilder for every FunctionDeclaration so that
//     forward calls and recursive calls can be resolved.
//
//   Pass 2 — Function body emission:
//     Emit IL into each registered MethodBuilder.
//
//   Pass 3 — Main emission:
//     Emit all top-level non-function expressions into Main.
//     Each expression's value is discarded — Main is void.
public class Compiler
{
    private static void RegisterPrecompiledFunctions(EmitContext ctx)
    {
        var t = typeof(Helpers.PrecompiledFunctions);
        ctx.PrecompiledFunctions["head"]    = t.GetMethod(nameof(Helpers.PrecompiledFunctions.Head))!;
        ctx.PrecompiledFunctions["tail"]    = t.GetMethod(nameof(Helpers.PrecompiledFunctions.Tail))!;
        ctx.PrecompiledFunctions["last"]    = t.GetMethod(nameof(Helpers.PrecompiledFunctions.Last))!;
        ctx.PrecompiledFunctions["len"]     = t.GetMethod(nameof(Helpers.PrecompiledFunctions.Len))!;
        ctx.PrecompiledFunctions["empty"]   = t.GetMethod(nameof(Helpers.PrecompiledFunctions.Empty))!;
        ctx.PrecompiledFunctions["nth"]     = t.GetMethod(nameof(Helpers.PrecompiledFunctions.Nth))!;
        ctx.PrecompiledFunctions["reverse"] = t.GetMethod(nameof(Helpers.PrecompiledFunctions.Reverse))!;
        ctx.PrecompiledFunctions["concat"]  = t.GetMethod(nameof(Helpers.PrecompiledFunctions.Concat))!;
        ctx.PrecompiledFunctions["str"]     = t.GetMethod(nameof(Helpers.PrecompiledFunctions.Str))!;
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
            var adapters  = new Dictionary<string, MethodBuilder>();

            // ============================
            // Pass 1 — Register module function signatures (prefixed)
            // ============================
            foreach (var (moduleName, moduleExprs) in modules ?? [])
                foreach (var fn in moduleExprs.OfType<FunctionDeclaration>())
                    FunctionEmitter.Define(typeBuilder, fn, functions, adapters, prefix: moduleName + ".");

            // ============================
            // Pass 1 — Register main function signatures
            // ============================
            foreach (var fn in expressions.OfType<FunctionDeclaration>())
                FunctionEmitter.Define(typeBuilder, fn, functions, adapters);

            var ctx = new EmitContext(functions, adapters, typeMap);
            RegisterPrecompiledFunctions(ctx);

            // ============================
            // Pass 2 — Emit module function bodies
            // ============================
            foreach (var (moduleName, moduleExprs) in modules ?? [])
                foreach (var fn in moduleExprs.OfType<FunctionDeclaration>())
                    FunctionEmitter.Emit(fn, ctx, prefix: moduleName + ".");

            // ============================
            // Pass 2 — Emit main function bodies
            // ============================
            foreach (var fn in expressions.OfType<FunctionDeclaration>())
                FunctionEmitter.Emit(fn, ctx);

            // ============================
            // Pass 3 — Emit Main
            // ============================
            var il = methodBuilder.GetILGenerator();

            foreach (var expr in expressions.Where(e => e is not FunctionDeclaration and not ImportDeclaration))
            {
                ExpressionEmitter.EmitToStack(il, expr, ctx);
                il.Emit(OpCodes.Pop);
            }

            // If `main` is declared, call it as the entry point after all top-level expressions.
            if (ctx.Functions.TryGetValue("main", out var userMain))
            {
                il.Emit(OpCodes.Call, userMain);
                il.Emit(OpCodes.Pop);
            }

            il.Emit(OpCodes.Ret);

            // ============================
            // Finalize and invoke
            // ============================
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
