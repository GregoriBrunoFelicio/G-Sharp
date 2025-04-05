using System.Reflection.Emit;
using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.CodeGen;

public static class ExpressionEmitter
{
    public static LocalBuilder Emit(ILGenerator il, Expression expression, Dictionary<string, LocalBuilder> locals)
    {
        return expression switch
        {
            LiteralExpression lit => EmitLiteral(il, lit.Value),
            VariableExpression v => locals[v.Name],
            _ => throw new Exception("Unsupported expression")
        };
    }

    private static LocalBuilder EmitLiteral(ILGenerator il, VariableValue value)
    {
        return value switch
        {
            IntValue i => NumberEmitter.EmitInt(il, i.Value),
            DoubleValue d => NumberEmitter.EmitDouble(il, d.Value),
            FloatValue f => NumberEmitter.EmitFloat(il, f.Value),
            BooleanValue b => BooleanEmitter.Emit(il, b.Value),
            StringValue s => StringEmitter.Emit(il, s.Value),
            ArrayValue a => ArrayEmitter.Emit(il, a),
            _ => throw new NotSupportedException($"Unsupported literal: {value.GetType().Name}")
        };
    }
}