using System.Reflection;
using System.Reflection.Emit;
using G.Sharp.Compiler.AST;
using Type = System.Type;

namespace G.Sharp.Compiler;

public class Compiler
{
    private readonly Dictionary<string, LocalBuilder> _locals = new();

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
        var main = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
        if (main == null)
            throw new Exception("Method 'Main' was not found.");
        main.Invoke(null, null);
    }

    private void EmitLetStatement(ILGenerator il, LetStatement stmt)
    {
        var local = stmt.VariableValue switch
        {
            IntValue numberInt => EmitInt(il, numberInt.Value),
            FloatValue numberFloat => EmitFloat(il, numberFloat.Value),
            DoubleValue numberDouble => EmitDouble(il, numberDouble.Value),
            DecimalValue numberDecimal => EmitDecimal(il, numberDecimal.Value),
            StringValue str => EmitString(il, str.Value),
            BooleanValue boolean => EmitBool(il, boolean.Value),
            _ => throw new NotSupportedException($"Unsupported value type: {stmt.VariableValue.GetType().Name}")
        };

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

    private static LocalBuilder EmitString(ILGenerator il, string value)
    {
        il.Emit(OpCodes.Ldstr, value);
        var local = il.DeclareLocal(typeof(string));
        il.Emit(OpCodes.Stloc, local);
        return local;
    }

    private static LocalBuilder EmitBool(ILGenerator il, bool value)
    {
        il.Emit(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        var local = il.DeclareLocal(typeof(bool));
        il.Emit(OpCodes.Stloc, local);
        return local;
    }


    /// <summary>
    /// Emits an integer value to the IL generator and stores it in a local variable.
    /// </summary>
    private static LocalBuilder EmitInt(ILGenerator il, int value)
    {
        il.Emit(OpCodes.Ldc_I4, value);
        var local = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Stloc, local);
        return local;
    }
    
    private static LocalBuilder EmitFloat(ILGenerator il, float value)
    {
        il.Emit(OpCodes.Ldc_R4, value);
        var local = il.DeclareLocal(typeof(float));
        il.Emit(OpCodes.Stloc, local);
        return local;
    }

    private static LocalBuilder EmitDouble(ILGenerator il, double value)
    {
        il.Emit(OpCodes.Ldc_R8, value);
        var local = il.DeclareLocal(typeof(double));
        il.Emit(OpCodes.Stloc, local);
        return local;
    }

    private static LocalBuilder EmitDecimal(ILGenerator il, decimal value)
    {
        var local = il.DeclareLocal(typeof(decimal));
        
        var ctor = typeof(decimal).GetConstructor([typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte)]);

        var bits = decimal.GetBits(value);
        var lo = bits[0];
        var mid = bits[1];
        var hi = bits[2];
        var sign = (bits[3] & 0x80000000) != 0;
        var scale = (byte)((bits[3] >> 16) & 0x7F);

        il.Emit(OpCodes.Ldc_I4, lo);
        il.Emit(OpCodes.Ldc_I4, mid);
        il.Emit(OpCodes.Ldc_I4, hi);
        il.Emit(sign ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ldc_I4_S, scale);
        il.Emit(OpCodes.Newobj, ctor);
        il.Emit(OpCodes.Stloc, local);

        return local;
    }
}