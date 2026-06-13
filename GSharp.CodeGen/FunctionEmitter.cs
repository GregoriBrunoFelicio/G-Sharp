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
//                    Also registers an adapter method for higher-order use.
//
//   Pass 2 (Emit):   emit the actual IL into each MethodBuilder and its adapter.
//
// All G# functions have the signature: static object Name(object p1, object p2, ...)
// The dynamic type system lives entirely at runtime via RuntimeHelpers.
//
// For higher-order use, each function also gets an adapter:
//   static object Name__adapter(object[] args) => Name(args[0], args[1], ...)
// The adapter is wrapped in a GSharpFunction when the function is used as a value.
//
// Return value convention:
//   - The last expression in the body is the implicit return value.
//   - All previous expressions have their values discarded with Pop.
//   - Empty bodies return null.
public static class FunctionEmitter
{
    // Pass 1: define the MethodBuilder for a function declaration.
    //
    // Registers the function's name and signature in the 'functions' dictionary
    // so that CallExpression nodes can reference it via context.Functions[name].
    // The body is not emitted here — that happens in Pass 2.
    //
    // All parameters are object because G# is dynamically typed.
    // Return type is always object for the same reason.
    public static void Define(
        TypeBuilder typeBuilder,
        FunctionDeclaration fn,
        Dictionary<string, MethodBuilder> functions,
        Dictionary<string, MethodBuilder> adapters,
        string prefix = "")
    {
        var qualifiedName = prefix + fn.Name;
        var paramTypes = Enumerable.Repeat(typeof(object), fn.Parameters.Count).ToArray();

        var method = typeBuilder.DefineMethod(
            qualifiedName,
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(object),
            paramTypes);

        functions[qualifiedName] = method;

        var adapter = typeBuilder.DefineMethod(
            qualifiedName + "__adapter",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(object),
            [typeof(object[])]);

        adapters[qualifiedName] = adapter;
    }

    // Pass 2: emit IL into the MethodBuilder registered in Pass 1.
    //
    // A fresh EmitContext is created for each function body so that its locals
    // are separate from Main's locals and other functions' locals, and its
    // parameters are registered so that IdentifierExpression nodes emit Ldarg
    // instead of Ldloc for parameter names.
    public static void Emit(FunctionDeclaration fn, EmitContext context, string prefix = "")
    {
        EmitMainBody(fn, context, prefix);
        EmitAdapter(fn, context, prefix);
    }

    private static void EmitMainBody(FunctionDeclaration fn, EmitContext context, string prefix)
    {
        var qualifiedName = prefix + fn.Name;
        var method = context.Functions[qualifiedName];
        var il = method.GetILGenerator();

        var functionContext = new EmitContext(context.Functions, context.FunctionAdapters, context.TypeMap);
        foreach (var (builtinName, builtinMethod) in context.Builtins)
            functionContext.Builtins[builtinName] = builtinMethod;

        // Register each parameter name → argument index.
        // The CLR accesses arguments via Ldarg_0, Ldarg_1, etc. — not Ldloc.
        for (var i = 0; i < fn.Parameters.Count; i++)
            functionContext.Parameters[fn.Parameters[i]] = i;

        var body = fn.Body;

        // Emit all expressions except the last — discard their values.
        for (var i = 0; i < body.Count - 1; i++)
        {
            ExpressionEmitter.EmitToStack(il, body[i], functionContext);
            il.Emit(OpCodes.Pop);
        }

        // Emit the last expression — its value is the implicit return value.
        if (body.Count > 0)
            ExpressionEmitter.EmitToStack(il, body[^1], functionContext);
        else
            il.Emit(OpCodes.Ldnull);

        // Ret pops the top of the stack as the return value.
        il.Emit(OpCodes.Ret);
    }

    // Emits the adapter body: unpacks object[] and calls the main static method.
    //
    //   static object Name__adapter(object[] args) => Name(args[0], args[1], ...)
    //
    // This adapter is used when the function is passed as a first-class value.
    // It bridges the GSharpFunction(object[] args) calling convention with the
    // strongly-typed static method signature.
    private static void EmitAdapter(FunctionDeclaration fn, EmitContext context, string prefix)
    {
        var qualifiedName = prefix + fn.Name;
        var adapter = context.FunctionAdapters[qualifiedName];
        var il = adapter.GetILGenerator();

        for (var i = 0; i < fn.Parameters.Count; i++)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, i);
            il.Emit(OpCodes.Ldelem_Ref);
        }

        il.Emit(OpCodes.Call, context.Functions[qualifiedName]);
        il.Emit(OpCodes.Ret);
    }
}
