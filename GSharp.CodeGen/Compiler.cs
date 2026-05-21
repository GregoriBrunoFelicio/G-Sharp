using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
using Type = System.Type;

namespace GSharp.CodeGen;

// Entry point for the G# compiler.
//
// Receives a fully-parsed and validated AST and compiles it to .NET IL
// using System.Reflection.Emit. The compiled code runs in-memory —
// no files are written to disk.
//
// Compilation happens in three passes:
//
//   Pass 1 — Function registration:
//     Define a MethodBuilder for every function declaration so that
//     forward calls (calling a function before its definition) and
//     recursive calls can be resolved. At this stage, only the method
//     signatures are registered — no IL is emitted yet.
//
//   Pass 2 — Function body emission:
//     Emit IL into each registered MethodBuilder. Because all functions
//     are already registered from pass 1, calls between them resolve cleanly.
//
//   Pass 3 — Main emission:
//     Emit IL for all top-level non-function statements into the Main method.
//     This is the program's entry point.
//
// After all three passes, the TypeBuilder is finalized with CreateType(),
// which locks it and makes the generated code invocable via reflection.
public class Compiler
{
    // Creates the dynamic assembly infrastructure that hosts the compiled code.
    //
    // The hierarchy is: AssemblyBuilder → ModuleBuilder → TypeBuilder → MethodBuilder.
    // We create one assembly, one module, one "Program" type, and one "Main" method.
    // User-defined functions are added as additional static methods on the same type.
    //
    // AssemblyBuilderAccess.Run means the assembly exists only in memory.
    // It cannot be saved to disk with this flag.
    private static (MethodBuilder, TypeBuilder) CreateBuilders()
    {
        var assemblyName = new AssemblyName("GSharpRuntimeAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            assemblyName, AssemblyBuilderAccess.Run);

        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

        // Define a "Program" type to host all generated methods.
        var typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Public);

        // Define the Main method: public static void Main()
        // This is the program's entry point — top-level G# statements go here.
        var methodBuilder = typeBuilder.DefineMethod(
            "Main",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(void),
            Type.EmptyTypes);

        return (methodBuilder, typeBuilder);
    }

    public void CompileAndRun(List<Statement> statements)
    {
        try
        {
            var (methodBuilder, typeBuilder) = CreateBuilders();

            // All function MethodBuilders are collected here.
            // Populated in pass 1 so passes 2 and 3 can resolve calls.
            var functions = new Dictionary<string, MethodBuilder>();

            // ============================
            // Pass 1 — Register all function signatures
            // ============================
            // We scan the entire statement list before emitting any IL.
            // This means a function defined at line 100 can be called at line 5.
            foreach (var fn in statements.OfType<FunctionDeclaration>())
                FunctionEmitter.Define(typeBuilder, fn, functions);

            // The EmitContext carries locals, parameters, and the functions map.
            // It is shared across passes — functions added in pass 1 are visible in pass 2 and 3.
            var ctx = new EmitContext(functions);

            // ============================
            // Pass 2 — Emit function bodies
            // ============================
            // Each function gets its own ILGenerator (from its MethodBuilder).
            // FunctionEmitter creates a child EmitContext with the function's own
            // locals and parameter bindings so they don't interfere with Main.
            foreach (var fn in statements.OfType<FunctionDeclaration>())
                FunctionEmitter.Emit(fn, ctx);

            // ============================
            // Pass 3 — Emit Main
            // ============================
            // Top-level statements that are not function declarations go into Main.
            var il = methodBuilder.GetILGenerator();
            foreach (var stmt in statements.Where(s => s is not FunctionDeclaration))
                StatementEmitter.Emit(il, stmt, ctx);

            // Every IL method must end with Ret.
            // Main returns void so nothing is pushed before Ret.
            il.Emit(OpCodes.Ret);

            // ============================
            // Finalize and invoke
            // ============================
            // CreateType() "seals" the type — no more methods or fields can be added.
            // After this call, the generated IL is compiled to native code by the JIT
            // the first time Main is invoked.
            var programType = typeBuilder.CreateType();

            var main = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static)
                ?? throw new Exception("Method 'Main' was not found.");

            // Execute the compiled program.
            main.Invoke(null, null);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.InnerException?.Message ?? exception.Message);
            throw;
        }
    }
}
