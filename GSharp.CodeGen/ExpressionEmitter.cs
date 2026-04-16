using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;
using GSharp.Lexer;

namespace GSharp.CodeGen;

// This class is responsible for emitting IL for expressions.
//
// VERY IMPORTANT RULE OF THIS COMPILER:
//
// - Every expression MUST leave exactly ONE *object* on the IL stack.
// - Even booleans are boxed before leaving this class.
// - Control flow (if/while) will later decide how to interpret the value.
//
// If you break this rule, random IL errors WILL happen.
public static class ExpressionEmitter
{
    // Entry point for emitting expressions.
    //
    // This method guarantees that after it runs,
    // the evaluation stack contains exactly ONE object.
    public static void EmitToStack(
        ILGenerator il,
        Expression expression,
        Dictionary<string, LocalBuilder> locals)
    {
        // Pattern-match the AST expression type.
        // At this point, parsing and validation already happened.
        switch (expression)
        {
            // ============================
            // Literal values (numbers, strings, bool, arrays, etc.)
            // ============================
            case LiteralExpression lit:
                EmitLiteral(il, lit.Value);
                break;

            // ============================
            // Variable access
            // ============================
            case VariableExpression v:
                // Load the local variable directly.
                // Locals are always stored as object.
                il.Emit(OpCodes.Ldloc, locals[v.Name]);
                break;

            // ============================
            // Binary expressions (a + b, a == b, a and b, etc.)
            // ============================
            case BinaryExpression b:
                // AND / OR are NOT normal binary operators.
                // They require short-circuit evaluation.
                if (b.Operator is TokenType.And or TokenType.Or)
                {
                    // Emit the short-circuit logic.
                    // This returns a LocalBuilder holding a bool (value type).
                    var result = EmitLogicalShortCircuit(il, b, locals);

                    // Load the boolean result onto the stack.
                    il.Emit(OpCodes.Ldloc, result);

                    // VERY IMPORTANT:
                    // We MUST box the bool here.
                    // The language contract says:
                    // "Every expression returns object".
                    il.Emit(OpCodes.Box, typeof(bool));
                }
                else
                {
                    // All other binary operators are handled normally.
                    // These usually call RuntimeHelpers.
                    EmitBinary(il, b, locals);
                }
                break;

            // ============================
            // Safety net
            // ============================
            default:
                throw new NotSupportedException(
                    $"Unsupported expression: {expression.GetType().Name}");
        }
    }

    // Emits IL for "normal" binary operators.
    //
    // Examples:
    // - a + b
    // - a == b
    // - a < b
    //
    // These operators:
    // - always evaluate BOTH sides
    // - delegate the logic to RuntimeHelpers
    // - leave an object on the stack
    private static void EmitBinary(
        ILGenerator il,
        BinaryExpression expr,
        Dictionary<string, LocalBuilder> locals)
    {
        // Emit left-hand side.
        // Leaves object on stack.
        EmitToStack(il, expr.Left, locals);

        // Emit right-hand side.
        // Leaves object on stack.
        EmitToStack(il, expr.Right, locals);

        // Pick the correct runtime helper based on the operator.
        // This keeps the emitter simple and moves logic to runtime.
        var method = expr.Operator switch
        {
            TokenType.GreaterThan =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.GreaterThan)),
            TokenType.LessThan =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.LessThan)),
            TokenType.GreaterThanOrEqual =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.GreaterThanOrEqual)),
            TokenType.LessThanOrEqual =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.LessThanOrEqual)),
            TokenType.EqualEqual =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.EqualEqual)),
            TokenType.NotEqual =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.NotEqual)),
            TokenType.Plus =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Add)),
            TokenType.Minus =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Subtract)),
            TokenType.Multiply =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Multiply)),
            TokenType.Divide =>
                typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Divide)),
            _ => throw new NotSupportedException(expr.Operator.ToString())
        };

        // Call the runtime helper.
        // It is responsible for:
        // - dynamic typing
        // - type checks
        // - returning the correct object
        il.Emit(OpCodes.Call, method!);
    }

    // Emits IL for literal values.
    //
    // Literals are turned into CLR values and then boxed
    // to respect the dynamic language contract.
    private static void EmitLiteral(ILGenerator il, object value)
    {
        switch (value)
        {
            case int i:
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Box, typeof(int));
                break;

            case double d:
                il.Emit(OpCodes.Ldc_R8, d);
                il.Emit(OpCodes.Box, typeof(double));
                break;

            case bool b:
                il.Emit(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Box, typeof(bool));
                break;

            case string s:
                // Strings are already reference types.
                // No boxing needed.
                il.Emit(OpCodes.Ldstr, s);
                break;
            
            case float f:
                il.Emit(OpCodes.Ldc_R4, f);
                il.Emit(OpCodes.Box, typeof(float));
                break;
            
            case decimal m:
                // Decimal is special: it has no IL literal opcode.
                // We manually construct it.
                EmitDecimal(il, m);
                il.Emit(OpCodes.Box, typeof(decimal));
                break;
            
            case object[] arr:
                // Array literals are handled by ArrayEmitter.
                // Arrays are always object[].
                ArrayEmitter.EmitToStack(il, arr);
                break;

            default:
                throw new NotSupportedException(
                    $"Unsupported literal: {value?.GetType().Name}");
        }
    }

    // Emits IL to construct a decimal value.
    //
    // Decimal has a weird internal representation,
    // so we need to manually call its constructor.
    private static void EmitDecimal(ILGenerator il, decimal value)
    {
        var bits = decimal.GetBits(value);

        var lo = bits[0];
        var mid = bits[1];
        var hi = bits[2];

        var sign = (bits[3] & 0x80000000) != 0;
        var scale = (byte)((bits[3] >> 16) & 0x7F);

        var ctor = typeof(decimal).GetConstructor([
            typeof(int), typeof(int), typeof(int),
            typeof(bool), typeof(byte)
        ]) ?? throw new Exception("Decimal constructor not found");

        il.Emit(OpCodes.Ldc_I4, lo);
        il.Emit(OpCodes.Ldc_I4, mid);
        il.Emit(OpCodes.Ldc_I4, hi);
        il.Emit(sign ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ldc_I4_S, scale);

        // new decimal(lo, mid, hi, sign, scale)
        il.Emit(OpCodes.Newobj, ctor);
    }
    
    // Emits short-circuit logic for AND / OR.
    //
    // This is NOT a normal binary operation.
    // It is basically control flow disguised as an expression.
    //
    // IMPORTANT:
    // - This method DOES NOT leave a value on the stack.
    // - It RETURNS a LocalBuilder holding a bool.
    // - The caller decides how to use and box the result.
    private static LocalBuilder EmitLogicalShortCircuit(
        ILGenerator il,
        BinaryExpression expr,
        Dictionary<string, LocalBuilder> locals)
    {
        // Local variable to store the boolean result.
        var result = il.DeclareLocal(typeof(bool));

        // Labels for short-circuit and exit.
        var shortCircuit = il.DefineLabel();
        var end = il.DefineLabel();

        // ============================
        // Evaluate left side
        // ============================
        EmitToStack(il, expr.Left, locals);

        // Left side returns object.
        // We MUST unbox it to bool to make decisions.
        il.Emit(OpCodes.Unbox_Any, typeof(bool));

        // true || x -> true (skip right side)
        // false && x  -> false (skip right side)
        il.Emit(expr.Operator == TokenType.And ? OpCodes.Brfalse : OpCodes.Brtrue, shortCircuit);

        // ============================
        // Evaluate right side (only if needed)
        // ============================
        EmitToStack(il, expr.Right, locals);
        il.Emit(OpCodes.Unbox_Any, typeof(bool));
        il.Emit(OpCodes.Stloc, result);
        il.Emit(OpCodes.Br, end);
        
        // ============================
        // Short-circuit path
        // ============================
        il.MarkLabel(shortCircuit);

        // Push the short-circuit result:
        // AND -> false
        // OR  -> true
        il.Emit(expr.Operator == TokenType.And
            ? OpCodes.Ldc_I4_0
            : OpCodes.Ldc_I4_1);

        il.Emit(OpCodes.Stloc, result);

        // ============================
        // End
        // ============================
        il.MarkLabel(end);

        return result;
    }
}
