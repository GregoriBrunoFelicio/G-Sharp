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
        // Empilha o array (não cria variável)
        ExpressionEmitter.EmitToStack(il, statement.Iterable, variables);

        // Cria variável local para o array
        var arrayType = GetExpressionClrType(statement.Iterable, variables);

        if (!arrayType.IsArray)
            throw new InvalidOperationException($"Expected array expression in 'for', got: {arrayType}");

        var arrayLocal = il.DeclareLocal(arrayType);
        il.Emit(OpCodes.Stloc, arrayLocal);

        // int i = 0;
        var indexLocal = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc, indexLocal);

        var loopStart = il.DefineLabel();
        var loopEnd = il.DefineLabel();

        il.MarkLabel(loopStart);

        // if (i >= array.Length) goto loopEnd;
        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Conv_I4);
        il.Emit(OpCodes.Bge, loopEnd);

        // var item = array[i];
        var elementType = arrayType.GetElementType()
            ?? throw new InvalidOperationException($"Unable to get element type from array: {arrayType}");

        var loopVar = il.DeclareLocal(elementType);
        variables[statement.Variable] = loopVar;

        il.Emit(OpCodes.Ldloc, arrayLocal);
        il.Emit(OpCodes.Ldloc, indexLocal);
        il.Emit(GetLdelemOpCode(elementType));
        il.Emit(OpCodes.Stloc, loopVar);

        // Loop body
        foreach (var s in statement.Body)
        {
            StatementEmitter.Emit(il, s, variables);
        }

        // i++
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
        throw new NotSupportedException($"Unsupported array element type: {type}");
    }

    private static Type GetExpressionClrType(Expression expression, Dictionary<string, LocalBuilder> locals)
    {
        return expression switch
        {
            LiteralExpression lit => lit.Value.Type.GetClrType(),
            VariableExpression v when locals.TryGetValue(v.Name, out var local) => local.LocalType,
            _ => throw new NotSupportedException($"Cannot determine CLR type for expression: {expression}")
        };
    }
}