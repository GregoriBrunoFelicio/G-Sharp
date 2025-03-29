using System.Reflection;
using System.Reflection.Emit;
using G.Sharp.Compiler.AST;
using Type = System.Type;

namespace G.Sharp.Compiler;

public class Compiler
{
    private readonly Dictionary<string, LocalBuilder> _locals = new();

    public void CompileAndRun(List<Statement> statements)
    {
        var (methodBuilder, typeBuilder) = CreateBuilders();
        var il = methodBuilder.GetILGenerator();

        foreach (var statement in statements)
        {
            switch (statement)
            {
                case LetStatement letStmt:
                    EmitLetStatement(il, letStmt);
                    break;

                case PrintStatement printStmt:
                    EmitPrintStatement(il, printStmt);
                    break;

                default:
                    throw new NotSupportedException($"Unsupported statement type: {statement.GetType().Name}");
            }
        }

        il.Emit(OpCodes.Ret);

        var programType = typeBuilder.CreateType();
        var main = programType.GetMethod("Main",  BindingFlags.Public | BindingFlags.Static);
        if (main == null)
            throw new Exception("Method 'Main' was not found.");
        main.Invoke(null, null);
    }

    private void EmitLetStatement(ILGenerator il, LetStatement stmt)
    {
        var local = stmt.VariableValue switch
        {
            NumberValue numberIntValue => DeclareAndLoadValue(il, typeof(int),
                () => il.Emit(OpCodes.Ldc_I4, numberIntValue.Value)),
            StringValue stringValue => DeclareAndLoadValue(il, typeof(string),
                () => il.Emit(OpCodes.Ldstr, stringValue.Value)),
            BooleanValue booleanValue => DeclareAndLoadValue(il, typeof(bool),
                () => il.Emit(booleanValue.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0)),
            _ => throw new NotSupportedException($"Unsupported value type: {stmt.VariableValue.GetType().Name}")
        };

        il.Emit(OpCodes.Stloc, local);
        _locals[stmt.VariableName] = local;
    }

    private void EmitPrintStatement(ILGenerator il, PrintStatement stmt)
    {
        if (!_locals.TryGetValue(stmt.VariableName, out var variable))
            throw new InvalidOperationException($"Variable '{stmt.VariableName}' is not defined.");

        il.Emit(OpCodes.Ldloc, variable);

        var method = typeof(Console).GetMethod(
            "WriteLine",
            BindingFlags.Public | BindingFlags.Static,
            null,
            [variable.LocalType],
            null);

        if (method == null)
            throw new MissingMethodException(
                $"No suitable Console.WriteLine method found for type '{variable.LocalType.Name}'.");

        il.Emit(OpCodes.Call, method);
    }

    private static LocalBuilder DeclareAndLoadValue(ILGenerator il, Type type, Action emitLoad)
    {
        var local = il.DeclareLocal(type);
        emitLoad();
        return local;
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