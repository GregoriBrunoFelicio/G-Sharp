using System.Reflection;
using System.Reflection.Emit;
using G.Sharp.Compiler.AST;
using Type = System.Type;

namespace G.Sharp.Compiler;

public static class Compiler
{
    public static void CompileAndRun(List<Statement> statements)
    {
        var (methodBuilder, typeBuilder) = CreateBuilders();
        var il = methodBuilder.GetILGenerator();
        var locals = new Dictionary<string, LocalBuilder>();

        foreach (var stmt in statements)
        {
            switch (stmt)
            {
                case LetStatement letStmt:
                {
                    LocalBuilder local;
                    switch (letStmt.VariableValue)
                    {
                        case NumberValue nv:
                            local = il.DeclareLocal(typeof(int));
                            il.Emit(OpCodes.Ldc_I4, nv.Value);
                            break;

                        case StringValue sv:
                            local = il.DeclareLocal(typeof(string));
                            il.Emit(OpCodes.Ldstr, sv.Value);
                            break;
                        case BooleanValue bv:
                            local = il.DeclareLocal(typeof(bool));
                            il.Emit(bv.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                            break;

                        default:
                            throw new Exception($"Unsupported value type: {letStmt.VariableValue.GetType().Name}");
                    }

                    il.Emit(OpCodes.Stloc, local);
                    locals[letStmt.VariableName] = local;
                    break;
                }

                case PrintStatement printStmt:
                {
                    if (!locals.TryGetValue(printStmt.VariableName, out var variable))
                        throw new Exception($"Undefined variable '{printStmt.VariableName}'");

                    il.Emit(OpCodes.Ldloc, variable);

                    var method = typeof(Console).GetMethod(
                        "WriteLine",
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        [variable.LocalType],
                        null);

                    if (method == null)
                        throw new Exception($"No suitable Console.Write method for type {variable.LocalType}");

                    il.Emit(OpCodes.Call, method);
                    break;
                }
            }
        }

        il.Emit(OpCodes.Ret);

        var programType = typeBuilder.CreateType();
        var main = programType.GetMethod("Main");
        main.Invoke(null, null);
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
}