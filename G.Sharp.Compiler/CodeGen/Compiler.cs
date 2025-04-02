using System.Reflection;
using System.Reflection.Emit;
using G.Sharp.Compiler.AST;
using Type = System.Type;

namespace G.Sharp.Compiler.CodeGen;

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
                
                case AssignmentStatement assignStmt:
                    EmitAssignmentStatement(il, assignStmt); 
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

    private void EmitLetStatement(ILGenerator il, LetStatement statement)
    {
        var local = statement.VariableValue switch
        {
            IntValue numberInt => EmitInt(il, numberInt.Value),
            FloatValue numberFloat => EmitFloat(il, numberFloat.Value),
            DoubleValue numberDouble => EmitDouble(il, numberDouble.Value),
            DecimalValue numberDecimal => EmitDecimal(il, numberDecimal.Value),
            StringValue str => EmitString(il, str.Value),
            BooleanValue boolean => EmitBool(il, boolean.Value),
            ArrayValue array => EmitArray(il, array),
            _ => throw new NotSupportedException($"Unsupported value type: {statement.VariableValue.GetType().Name}")
        };

        _locals[statement.VariableName] = local;
    }

    private void EmitPrintStatement(ILGenerator il, PrintStatement statement)
    {
        if (!_locals.TryGetValue(statement.VariableName, out var variable))
            throw new InvalidOperationException($"Variable '{statement.VariableName}' is not defined.");

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
    
    private void EmitAssignmentStatement(ILGenerator il, AssignmentStatement statement)
    {
        if (!_locals.TryGetValue(statement.VariableName, out var local))
            throw new InvalidOperationException($"Variable '{statement.VariableName}' is not defined.");

        switch (statement.VariableValue)
        {
            case IntValue intVal:
                il.Emit(OpCodes.Ldc_I4, intVal.Value);
                break;

            case FloatValue floatVal:
                il.Emit(OpCodes.Ldc_R4, floatVal.Value);
                break;

            case DoubleValue doubleVal:
                il.Emit(OpCodes.Ldc_R8, doubleVal.Value);
                break;

            case DecimalValue decVal:
                EmitDecimal(il, decVal.Value);
                return;

            case StringValue strVal:
                il.Emit(OpCodes.Ldstr, strVal.Value);
                break;

            case BooleanValue boolVal:
                il.Emit(boolVal.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                break;

            default:
                throw new NotSupportedException($"Unsupported value type: {statement.VariableValue.GetType().Name}");
        }

        il.Emit(OpCodes.Stloc, local);
    }
    
    private static LocalBuilder EmitArray(ILGenerator il, ArrayValue array)
    {
        var elementType = GetSystemType(array.ElementType);
        var local = il.DeclareLocal(elementType.MakeArrayType());

        il.Emit(OpCodes.Ldc_I4, array.Elements.Count);
        il.Emit(OpCodes.Newarr, elementType);

        for (var i = 0; i < array.Elements.Count; i++)
        {
            il.Emit(OpCodes.Dup); // Thinking about it :/
            il.Emit(OpCodes.Ldc_I4, i); 

            EmitElement(il, array.Elements[i]); 

            il.Emit(GetStelemOpCode(elementType)); 
        }

        il.Emit(OpCodes.Stloc, local);
        return local;
    }
    
    private static Type GetSystemType(GType gtype)
    {
        return gtype.Kind switch
        {
            GPrimitiveType.Number => typeof(int), // TODO: Add another number types like float, double, decimal
            GPrimitiveType.String => typeof(string),
            GPrimitiveType.Boolean => typeof(bool),
            _ => throw new NotSupportedException($"Unknown GType {gtype.Kind}")
        };
    }
    
    private static OpCode GetStelemOpCode(Type type)
    {
        if (type == typeof(int)) return OpCodes.Stelem_I4;
        if (type == typeof(float)) return OpCodes.Stelem_R4;
        if (type == typeof(double)) return OpCodes.Stelem_R8;
        if (type == typeof(string)) return OpCodes.Stelem_Ref;
        if (type == typeof(bool)) return OpCodes.Stelem_I1;
        if (type == typeof(decimal)) throw new NotSupportedException("Arrays of decimal are not supported"); // TODO: Implement this
    
        throw new NotSupportedException($"Unsupported array element type: {type}");
    }
    
    private static void EmitElement(ILGenerator il, VariableValue value)
    {
        switch (value)
        {
            case IntValue intVal:
                il.Emit(OpCodes.Ldc_I4, intVal.Value);
                break;
            case FloatValue floatVal:
                il.Emit(OpCodes.Ldc_R4, floatVal.Value);
                break;
            case DoubleValue doubleVal:
                il.Emit(OpCodes.Ldc_R8, doubleVal.Value);
                break;
            case DecimalValue decVal:
                EmitDecimal(il, decVal.Value);
                break;
            case StringValue strVal:
                il.Emit(OpCodes.Ldstr, strVal.Value);
                break;
            case BooleanValue boolVal:
                il.Emit(boolVal.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                break;
            default:
                throw new NotSupportedException($"Unsupported array element type: {value.GetType().Name}");
        }
    }
}