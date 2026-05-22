using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
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

    public void CompileAndRun(List<Expression> expressions)
    {
        try
        {
            var (methodBuilder, typeBuilder) = CreateBuilders();

            var functions = new Dictionary<string, MethodBuilder>();

            // ============================
            // Pass 1 — Register all function signatures
            // ============================
            foreach (var fn in expressions.OfType<FunctionDeclaration>())
                FunctionEmitter.Define(typeBuilder, fn, functions);

            var ctx = new EmitContext(functions);

            // ============================
            // Pass 2 — Emit function bodies
            // ============================
            foreach (var fn in expressions.OfType<FunctionDeclaration>())
                FunctionEmitter.Emit(fn, ctx);

            // ============================
            // Pass 3 — Emit Main
            // ============================
            var il = methodBuilder.GetILGenerator();

            foreach (var expr in expressions.Where(e => e is not FunctionDeclaration))
            {
                ExpressionEmitter.EmitToStack(il, expr, ctx);
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
