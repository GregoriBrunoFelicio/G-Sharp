using System.Reflection;
using System.Reflection.Emit;
using G.Sharp.Compiler.AST;
using Type = System.Type;

namespace G.Sharp.Compiler.CodeGen;

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
        var (methodBuilder, typeBuilder) = CreateBuilders();
        var il = methodBuilder.GetILGenerator();

        foreach (var statement in statements)
        {
            switch (statement)
            {
                case LetStatement letStmt:
                    EmitLet.Emit(il, letStmt, _locals);
                    break;
                
                case AssignmentStatement assignStmt:
                    EmitAssignment.Emit(il, assignStmt, _locals); 
                    break;

                case PrintStatement printStmt:
                    EmitPrint.Emit(il, printStmt, _locals);
                    break;
                
                default:
                    throw new NotSupportedException($"Unsupported statement type: {statement.GetType().Name}");
            }
        }

        il.Emit(OpCodes.Ret);

        var programType = typeBuilder.CreateType();
        var main = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
        if (main == null)
            throw new Exception("Method 'Main' was not found.");
        main.Invoke(null, null);
    }


}