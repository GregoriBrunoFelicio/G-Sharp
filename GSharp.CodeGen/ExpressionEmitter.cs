using System.Reflection.Emit;
using GSharp.AST;
using GSharp.CodeGen.Helpers;
using GSharp.Lexer;

namespace GSharp.CodeGen;

// Emits IL for all expression types in G#.
//
// ══════════════════════════════════════════════════════════════
//  CRITICAL STACK CONTRACT — READ BEFORE MODIFYING THIS CLASS
// ══════════════════════════════════════════════════════════════
//
// Every path through EmitToStack MUST leave exactly ONE boxed object
// on the IL evaluation stack when it returns.
//
// Why? Because every consumer of an expression (let, println, if, binary ops,
// function arguments) assumes exactly one value waiting on the stack.
// If you leave zero values, the consumer reads garbage.
// If you leave two values, the stack overflows and the runtime throws.
//
// Even booleans must be boxed before leaving this class — the language
// contract is "everything is object". Unboxing happens only when a specific
// operation explicitly needs a primitive (e.g. Brfalse, short-circuit logic).
//
// ══════════════════════════════════════════════════════════════
public static class ExpressionEmitter
{
    // Entry point: emits IL for any expression and guarantees one object on the stack.
    public static void EmitToStack(ILGenerator il, Expression expression, EmitContext ctx)
    {
        switch (expression)
        {
            // ============================
            // Literal values (numbers, strings, booleans, arrays)
            // ============================
            case LiteralExpression lit:
                EmitLiteral(il, lit.Value);
                break;

            // ============================
            // Binding and parameter access
            // ============================
            case BindingExpression v:
                // Parameters (e.g. 'a' in 'soma(a b)') are in argument slots.
                // Ldarg is cheaper than Ldloc and is how the CLR exposes them.
                if (ctx.Parameters.TryGetValue(v.Name, out var paramIndex))
                    il.Emit(OpCodes.Ldarg, paramIndex);
                else if (ctx.Locals.TryGetValue(v.Name, out var local))
                    // Regular 'let' bindings live in declared local slots.
                    il.Emit(OpCodes.Ldloc, local);
                else if (ctx.FunctionAdapters.TryGetValue(v.Name, out var adapter))
                    // Function used as a first-class value — wrap in GSharpFunction.
                    EmitFunctionValue(il, adapter);
                else
                    throw new Exception($"Undefined binding: '{v.Name}'");
                break;

            // ============================
            // Function calls: name(args)
            // ============================
            case CallExpression call:
                if (ctx.PrecompiledFunctions.TryGetValue(call.Callee, out var precompiled))
                {
                    // Pre-compiled function — call PrecompiledFunctions method directly.
                    foreach (var arg in call.Arguments)
                        EmitToStack(il, arg, ctx);
                    il.Emit(OpCodes.Call, precompiled);
                }
                else if (ctx.Functions.ContainsKey(call.Callee))
                {
                    // Direct call to a known static function — fast Call opcode.
                    foreach (var arg in call.Arguments)
                        EmitToStack(il, arg, ctx);

                    // The CLR pops the arguments and pushes the return value.
                    il.Emit(OpCodes.Call, ctx.Functions[call.Callee]);
                }
                else
                {
                    // Higher-order call — callee is a GSharpFunction in a local or parameter.
                    EmitDelegateCall(il, call, ctx);
                }
                break;

            // ============================
            // Binary expressions (a + b, a == b, a and b, etc.)
            // ============================
            case BinaryExpression b:
                // AND / OR are NOT normal binary operators.
                // They require short-circuit evaluation, which is control flow
                // disguised as an expression. They get special treatment.
                if (b.Operator is TokenType.And or TokenType.Or)
                {
                    // EmitLogicalShortCircuit does NOT leave a value on the stack.
                    // It returns a LocalBuilder holding the result as a raw bool.
                    var result = EmitLogicalShortCircuit(il, b, ctx);

                    // Load the bool result and box it to satisfy the stack contract.
                    il.Emit(OpCodes.Ldloc, result);
                    il.Emit(OpCodes.Box, typeof(bool));
                }
                else
                {
                    // All other binary operators evaluate both sides and
                    // delegate the operation to RuntimeHelpers.
                    EmitBinary(il, b, ctx);
                }
                break;

            // ============================
            // Side-effecting expressions (evaluate to null)
            // ============================
            case LetExpression letExpr:
                LetEmitter.Emit(il, letExpr, ctx);
                break;

            case PrintExpression printExpr:
                PrintEmitter.Emit(il, printExpr, ctx);
                break;

            case ForExpression forExpr:
                ForEmitter.Emit(il, forExpr, ctx);
                break;

            case WhileExpression whileExpr:
                WhileEmitter.Emit(il, whileExpr, ctx);
                break;

            // ============================
            // Conditional expression
            // ============================
            case IfExpression ifExpr:
                IfEmitter.EmitToStack(il, ifExpr, ctx);
                break;

            // ============================
            // Function declarations are handled by the compiler passes — no IL here.
            // ============================
            case FunctionDeclaration:
                il.Emit(OpCodes.Ldnull);
                break;

            default:
                throw new NotSupportedException(
                    $"internal error: no emitter for expression '{expression.GetType().Name}'");
        }
    }

    // Emits IL for "normal" binary operators (arithmetic, comparison).
    //
    // Both operands are evaluated first, then a RuntimeHelpers method is called
    // to perform the operation dynamically. This is where type coercion lives.
    //
    // Stack before call: [left: object, right: object]
    // Stack after call:  [result: object]
    private static void EmitBinary(ILGenerator il, BinaryExpression expr, EmitContext ctx)
    {
        // Emit left-hand side — leaves one object on stack.
        EmitToStack(il, expr.Left, ctx);

        // Emit right-hand side — leaves one object on stack.
        // Stack now has: [left, right]
        EmitToStack(il, expr.Right, ctx);

        // Dispatch to the correct RuntimeHelpers method.
        // Each helper pops both operands and pushes the result.
        var method = expr.Operator switch
        {
            TokenType.GreaterThan        => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.GreaterThan)),
            TokenType.LessThan           => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.LessThan)),
            TokenType.GreaterThanOrEqual => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.GreaterThanOrEqual)),
            TokenType.LessThanOrEqual    => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.LessThanOrEqual)),
            TokenType.EqualEqual         => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.EqualEqual)),
            TokenType.NotEqual           => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.NotEqual)),
            TokenType.Plus               => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Add)),
            TokenType.Minus              => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Subtract)),
            TokenType.Multiply           => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Multiply)),
            TokenType.Divide             => typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.Divide)),
            _ => throw new NotSupportedException($"internal error: no emitter for binary operator '{expr.Operator}'")
        };

        // Call the helper. It consumes [left, right] and leaves [result] on the stack.
        il.Emit(OpCodes.Call, method!);
    }

    // Emits IL to push a literal value onto the stack as a boxed object.
    //
    // Every value type (int, double, bool, float, decimal) must be boxed
    // because the language contract requires object on the stack.
    // Strings are reference types so they need no boxing.
    private static void EmitLiteral(ILGenerator il, object value)
    {
        switch (value)
        {
            case int i:
                il.Emit(OpCodes.Ldc_I4, i);       // push int32 literal
                il.Emit(OpCodes.Box, typeof(int)); // box to object
                break;

            case double d:
                il.Emit(OpCodes.Ldc_R8, d);           // push float64 literal
                il.Emit(OpCodes.Box, typeof(double));  // box to object
                break;

            case bool b:
                // IL has no bool literal — 1 means true, 0 means false.
                il.Emit(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Box, typeof(bool));
                break;

            case string s:
                // Strings are reference types — Ldstr pushes a reference directly.
                // No boxing needed.
                il.Emit(OpCodes.Ldstr, s);
                break;

            case float f:
                il.Emit(OpCodes.Ldc_R4, f);
                il.Emit(OpCodes.Box, typeof(float));
                break;

            case decimal m:
                // Decimal has no IL literal opcode — it must be constructed manually.
                EmitDecimal(il, m);
                il.Emit(OpCodes.Box, typeof(decimal));
                break;

            case object[] arr:
                // Array literals delegate to ArrayEmitter.
                // ArrayEmitter leaves one object[] reference on the stack.
                ArrayEmitter.EmitToStack(il, arr);
                break;

            default:
                throw new NotSupportedException(
                    $"internal error: no emitter for literal type '{value?.GetType().Name ?? "null"}'");
        }
    }

    // Emits IL to construct a decimal value from its bit components.
    //
    // Decimal is unusual: it has no direct IL opcode (unlike int, double, float).
    // We must decompose it into its four int fields and call the constructor:
    //   new decimal(int lo, int mid, int hi, bool isNegative, byte scale)
    //
    // decimal.GetBits() returns the four int32 words that make up the value.
    private static void EmitDecimal(ILGenerator il, decimal value)
    {
        var bits  = decimal.GetBits(value);
        var lo    = bits[0];
        var mid   = bits[1];
        var hi    = bits[2];
        var sign  = (bits[3] & 0x80000000) != 0;  // high bit of word 3 is the sign
        var scale = (byte)((bits[3] >> 16) & 0x7F); // bits 16–23 of word 3 are the scale

        var ctor = typeof(decimal).GetConstructor([
            typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte)
        ]) ?? throw new Exception("Decimal constructor not found");

        // Push the five constructor arguments onto the stack.
        il.Emit(OpCodes.Ldc_I4, lo);
        il.Emit(OpCodes.Ldc_I4, mid);
        il.Emit(OpCodes.Ldc_I4, hi);
        il.Emit(sign ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ldc_I4_S, scale);

        // Call new decimal(lo, mid, hi, sign, scale).
        // This pops the five arguments and pushes the constructed decimal.
        il.Emit(OpCodes.Newobj, ctor);
    }

    // Emits IL for short-circuit AND / OR evaluation.
    //
    // Short-circuit means: if the result can be determined from the left side alone,
    // the right side is NOT evaluated. This matches the semantics of most languages.
    //
    //   AND: if left is false → result is false (skip right)
    //   OR:  if left is true  → result is true  (skip right)
    //
    // This method uses control flow (labels and branches) to skip the right side.
    // It does NOT leave a value on the stack — instead it stores the result in a
    // local variable and returns it. The caller is responsible for loading and boxing it.
    //
    // Control flow structure (for AND):
    //
    //   <emit left>
    //   Unbox_Any bool
    //   Brfalse shortCircuit   ; left is false → short-circuit to false
    //   <emit right>
    //   Unbox_Any bool
    //   Stloc result
    //   Br end
    //   shortCircuit:
    //     Ldc_I4_0             ; result = false
    //     Stloc result
    //   end:
    //   (return result)
    private static LocalBuilder EmitLogicalShortCircuit(
        ILGenerator il, BinaryExpression expr, EmitContext ctx)
    {
        // Allocate a local to hold the boolean result of the operation.
        var result = il.DeclareLocal(typeof(bool));

        var shortCircuit = il.DefineLabel();
        var end          = il.DefineLabel();

        // ============================
        // Evaluate left side
        // ============================
        EmitToStack(il, expr.Left, ctx);

        // Left side returns a boxed object — unbox it to a raw bool
        // so that Brfalse/Brtrue can make a decision.
        il.Emit(OpCodes.Unbox_Any, typeof(bool));

        // For AND: if left is false, we already know the result is false.
        // For OR:  if left is true,  we already know the result is true.
        il.Emit(expr.Operator == TokenType.And ? OpCodes.Brfalse : OpCodes.Brtrue, shortCircuit);

        // ============================
        // Evaluate right side (only reached if short-circuit didn't fire)
        // ============================
        EmitToStack(il, expr.Right, ctx);
        il.Emit(OpCodes.Unbox_Any, typeof(bool));
        il.Emit(OpCodes.Stloc, result);
        il.Emit(OpCodes.Br, end);

        // ============================
        // Short-circuit path
        // ============================
        il.MarkLabel(shortCircuit);

        // The short-circuit result is always the value that triggered the skip:
        // AND short-circuited → result is false (0)
        // OR  short-circuited → result is true  (1)
        il.Emit(expr.Operator == TokenType.And ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Stloc, result);

        il.MarkLabel(end);
        return result;
    }

    // Wraps a static adapter method in a GSharpFunction so the function can be
    // passed as a first-class value.
    //
    // IL emitted:
    //   Ldnull                              ; null target (static method)
    //   Ldftn  Name__adapter                ; function pointer to the adapter
    //   Newobj Func<object[], object>::.ctor ; create the delegate
    //   Newobj GSharpFunction::..ctor       ; wrap in GSharpFunction
    private static void EmitFunctionValue(ILGenerator il, MethodBuilder adapter)
    {
        var funcType = typeof(Func<object[], object>);
        var funcCtor = funcType.GetConstructors()[0]; // (object target, nativeint method)
        var gsFuncCtor = typeof(GSharpFunction).GetConstructor([funcType])!;

        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Ldftn, adapter);
        il.Emit(OpCodes.Newobj, funcCtor);
        il.Emit(OpCodes.Newobj, gsFuncCtor);
    }

    // Emits a call through a GSharpFunction value held in a local or parameter.
    //
    // The callee is cast to GSharpFunction, arguments are packed into object[],
    // and GSharpFunction.Call(object[]) is invoked.
    //
    // IL structure:
    //   <load callee>              ; parameter or local holding the GSharpFunction
    //   Castclass GSharpFunction
    //   Ldc_I4  argc
    //   Newarr  object             ; new object[argc]
    //   (for each arg:)
    //     Dup
    //     Ldc_I4  i
    //     <emit arg>
    //     Stelem_Ref               ; array[i] = arg
    //   Callvirt GSharpFunction.Call(object[])
    private static void EmitDelegateCall(ILGenerator il, CallExpression call, EmitContext ctx)
    {
        // Load the GSharpFunction value.
        if (ctx.Parameters.TryGetValue(call.Callee, out var paramIdx))
            il.Emit(OpCodes.Ldarg, paramIdx);
        else if (ctx.Locals.TryGetValue(call.Callee, out var local))
            il.Emit(OpCodes.Ldloc, local);
        else
            throw new Exception($"Undefined function or binding: '{call.Callee}'");

        il.Emit(OpCodes.Castclass, typeof(GSharpFunction));

        // Build the object[] argument array.
        il.Emit(OpCodes.Ldc_I4, call.Arguments.Count);
        il.Emit(OpCodes.Newarr, typeof(object));

        for (var i = 0; i < call.Arguments.Count; i++)
        {
            il.Emit(OpCodes.Dup);          // keep array reference on stack
            il.Emit(OpCodes.Ldc_I4, i);   // index
            EmitToStack(il, call.Arguments[i], ctx);
            il.Emit(OpCodes.Stelem_Ref);   // array[i] = value
        }

        il.Emit(OpCodes.Callvirt, typeof(GSharpFunction).GetMethod(nameof(GSharpFunction.Call))!);
    }
}
