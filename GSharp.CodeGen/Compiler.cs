using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;
using Type = System.Type;

namespace GSharp.CodeGen;

public class Compiler
{
    private readonly Dictionary<string, LocalBuilder> _locals = new();

    // TODO: Refactor this method remove hardcoded values
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

    public void CompileAndRun(List<Statement> statements)
    {
        try
        {
            var (methodBuilder, typeBuilder) = CreateBuilders();
            var il = methodBuilder.GetILGenerator();

            foreach (var statement in statements)
            {
                StatementEmitter.Emit(il, statement, _locals);
            }

            il.Emit(OpCodes.Ret);

            var programType = typeBuilder.CreateType();
            var main = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
            if (main == null)
                throw new Exception("Method 'Main' was not found.");
            main.Invoke(null, null);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.InnerException?.Message);
            throw;
        }
    }
}