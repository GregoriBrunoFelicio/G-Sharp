using System.Reflection;
using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// Emits IL for user-defined function declarations.
//
// Functions are compiled in two passes to support forward calls and recursion:
//
//   Pass 1 (Define): register a MethodBuilder for each function so its
//                    signature is known before any body is emitted.
//
//   Pass 2 (Emit):   emit the actual IL into each MethodBuilder.
//
// All G# functions have the signature: static object Name(object p1, object p2, ...)
// The dynamic type system lives entirely at runtime via RuntimeHelpers.
//
// Return value convention:
//   - The last statement in the body must be an ExpressionStatement.
//   - That expression is the implicit return value — its result is left on
//     the stack and Ret pops it as the return value.
//   - If the last statement is NOT an expression (e.g. println), the function
//     returns null. This handles "void-like" functions.
//   - Non-tail ExpressionStatements have their stack value discarded with Pop.
public static class FunctionEmitter
{
    // Pass 1: define the MethodBuilder for a function declaration.
    //
    // This registers the function's name and signature in the 'functions' dictionary
    // so that CallExpression nodes can reference it via ctx.Functions[name].
    // The body is not emitted here — that happens in Pass 2.
    //
    // All parameters are object because G# is dynamically typed.
    // Return type is always object for the same reason.
    public static void Define(TypeBuilder typeBuilder, FunctionDeclaration fn, Dictionary<string, MethodBuilder> functions)
    {
        var paramTypes = Enumerable.Repeat(typeof(object), fn.Parameters.Count).ToArray();

        var method = typeBuilder.DefineMethod(
            fn.Name,
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(object),   // return type
            paramTypes);      // one object slot per parameter

        functions[fn.Name] = method;
    }

    // Pass 2: emit IL into the MethodBuilder registered in Pass 1.
    //
    // A fresh EmitContext is created for each function body so that:
    //   - Its locals are separate from Main's locals and other functions' locals.
    //   - Its parameters are registered so that VariableExpression nodes
    //     emit Ldarg instead of Ldloc for parameter names.
    public static void Emit(FunctionDeclaration fn, EmitContext ctx)
    {
        var method = ctx.Functions[fn.Name];
        var il = method.GetILGenerator();

        // Create a child context that shares the functions dictionary
        // (so this function can call other functions and itself recursively)
        // but has its own locals and parameter bindings.
        var fnCtx = new EmitContext(ctx.Functions);

        // Register each parameter name → argument index.
        // The CLR accesses arguments via Ldarg_0, Ldarg_1, etc.
        for (var i = 0; i < fn.Parameters.Count; i++)
            fnCtx.Parameters[fn.Parameters[i]] = i;

        var body = fn.Body;

        // ============================
        // Emit all statements except the last
        // ============================
        // These are "normal" statements. If any of them happens to be an
        // ExpressionStatement (unusual in practice), its stack value is popped
        // because we're not in tail position — the value goes nowhere.
        for (var i = 0; i < body.Count - 1; i++)
        {
            StatementEmitter.Emit(il, body[i], fnCtx);
            if (body[i] is ExpressionStatement)
                il.Emit(OpCodes.Pop); // discard value — not the return value
        }

        // ============================
        // Emit the last statement — implicit return
        // ============================
        if (body.Count > 0 && body[^1] is ExpressionStatement tail)
        {
            // The last expression IS the return value.
            // Emit it directly so it stays on the stack for Ret to consume.
            ExpressionEmitter.EmitToStack(il, tail.Expression, fnCtx);
        }
        else
        {
            // Last statement is not an expression (e.g. println, let, if).
            // Emit it normally, then push null as the return value.
            // This effectively makes the function return null ("void-like").
            if (body.Count > 0)
                StatementEmitter.Emit(il, body[^1], fnCtx);
            il.Emit(OpCodes.Ldnull);
        }

        // Return — Ret pops the top of the stack as the return value.
        il.Emit(OpCodes.Ret);
    }
}
