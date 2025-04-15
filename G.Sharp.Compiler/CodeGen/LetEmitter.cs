using System.Reflection.Emit;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.CodeGen;

public static class LetEmitter
{
    public static void Emit(ILGenerator il, LetStatement statement, Dictionary<string, LocalBuilder> locals)
    {
        var local = ExpressionEmitter.Emit(il, statement.Expression, locals);
        locals[statement.VariableName] = local;
    }
}