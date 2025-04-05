using System.Reflection.Emit;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.CodeGen;

public static class ForEmitter
{
    public static void Emit(
        ILGenerator il,
        ForStatement statement,
        Dictionary<string, LocalBuilder> variables)
    {
        var arrayLocal = ExpressionEmitter.Emit(il, statement.Iterable, variables);

        var indexLocal = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc, indexLocal);

        var loopStart = il.DefineLabel();
        var loopEnd = il.DefineLabel();

        il.MarkLabel(loopStart);

        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Conv_I4);
        il.Emit(OpCodes.Bge, loopEnd);

        var elementType = arrayLocal.LocalType.GetElementType(); // It can return null if the type is not an array?
        var loopVar = il.DeclareLocal(elementType);
        variables[statement.Variable] = loopVar;

        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldloc, indexLocal);
        
        var loadOpCode = GetLdelemOpCode(elementType);
        il.Emit(loadOpCode);
        
        il.Emit(OpCodes.Stloc, loopVar);

        foreach (var s in statement.Body)
        {
            StatementEmitter.Emit(il, s, variables);
        }

        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc, indexLocal);

        il.Emit(OpCodes.Br, loopStart);
        il.MarkLabel(loopEnd);
    }

    private static OpCode GetLdelemOpCode(Type type)
    {
        if (type == typeof(int)) return OpCodes.Ldelem_I4;
        if (type == typeof(float)) return OpCodes.Ldelem_R4;
        if (type == typeof(double)) return OpCodes.Ldelem_R8;
        if (type == typeof(string)) return OpCodes.Ldelem_Ref;
        if (type == typeof(bool)) return OpCodes.Ldelem_I1;
        if (type == typeof(decimal))
            throw new NotSupportedException("Arrays of decimal are not supported");

        throw new NotSupportedException($"Unsupported array element type: {type}");
    }
}