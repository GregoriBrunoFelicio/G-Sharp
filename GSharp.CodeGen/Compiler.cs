using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
using Type = System.Type;

namespace GSharp.CodeGen;

// This class is the entry point of the compiler.

public class Compiler
{
    // Stores all local variables for the current compilation.
    // Variable names are mapped to IL locals.
    private readonly Dictionary<string, LocalBuilder> _locals = new();

    // Creates the dynamic assembly, module, type and Main method.
    // This is very boilerplate-heavy and intentionally explicit,
    // so it's easy to see what's going on.
    // NOTE:
    // A lot of things are hardcoded here for now (names, signatures).
    private static (MethodBuilder, TypeBuilder) CreateBuilders()
    {
        // Define a dynamic assembly that only exists in memory.
        var assemblyName = new AssemblyName("GSharpRuntimeAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            assemblyName, AssemblyBuilderAccess.Run);

        // Define a single module.
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

        // Define a Program type to host the Main method.
        var typeBuilder = moduleBuilder.DefineType(
            "Program",
            TypeAttributes.Public);

        // Define the Main method:
        //   public static void Main()
        var methodBuilder = typeBuilder.DefineMethod(
            "Main",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(void),
            Type.EmptyTypes);

        return (methodBuilder, typeBuilder);
    }

    // Compiles the given statements and immediately runs them.
    // This method assumes:
    // - The AST is already valid
    // - Parsing and validation already happened
    public void CompileAndRun(List<Statement> statements)
    {
        try
        {
            // Create the IL builders.
            var (methodBuilder, typeBuilder) = CreateBuilders();

            // Get the IL generator for the Main method.
            var il = methodBuilder.GetILGenerator();

            // Emit IL for each top-level statement.
            foreach (var statement in statements)
            {
                StatementEmitter.Emit(il, statement, _locals);
            }

            // Finish the method.
            il.Emit(OpCodes.Ret);

            // Finalize the Program type.
            var programType = typeBuilder.CreateType();

            // Resolve the generated Main method.
            var main = programType.GetMethod(
                "Main",
                BindingFlags.Public | BindingFlags.Static);

            if (main == null)
                throw new Exception("Method 'Main' was not found.");

            // Execute the generated code.
            main.Invoke(null, null);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.InnerException?.Message);
            throw;
        }
    }
}
