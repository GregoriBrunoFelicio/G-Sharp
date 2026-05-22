using System.Reflection.Emit;

namespace GSharp.CodeGen;

// Carries all the state an emitter needs to emit IL for a given scope.
//
// Before EmitContext existed, emitters received a raw Dictionary<string, LocalBuilder>
// and had no way to access function definitions or distinguish between local bindings
// and function parameters. EmitContext bundles all of that together in one place.
//
// Each function body gets its own EmitContext so that its locals and parameters
// are completely separate from Main's locals and from other functions.
public class EmitContext(
    Dictionary<string, MethodBuilder> functions,
    Dictionary<string, MethodBuilder> functionAdapters)
{
    // Maps binding names declared with 'let' to their IL local slots.
    // When the emitter encounters a BindingExpression, it looks here first.
    //
    // Example: 'let x = 10' adds "x" → LocalBuilder(slot 0).
    // Later, 'println x' emits Ldloc(slot 0) to push x onto the stack.
    public readonly Dictionary<string, LocalBuilder> Locals = new();

    // Maps function parameter names to their argument index (0-based).
    // Parameters are accessed via Ldarg, not Ldloc, because they live in
    // the argument slots of the method frame, not in declared locals.
    //
    // Example: 'soma(a b)' registers "a" → 0, "b" → 1.
    // Inside the body, 'a + b' emits Ldarg(0), Ldarg(1).
    //
    // This is empty for Main — Main has no parameters.
    public readonly Dictionary<string, int> Parameters = new();

    // Maps function names to their MethodBuilder (direct call target).
    // Populated in pass 1 so that forward calls and recursive calls resolve cleanly.
    public readonly Dictionary<string, MethodBuilder> Functions = functions;

    // Maps function names to their adapter MethodBuilder.
    // Adapters have signature: static object Name__adapter(object[] args)
    // They unpack the object[] and delegate to the main static method.
    // Used when a function is referenced as a value (higher-order use).
    public readonly Dictionary<string, MethodBuilder> FunctionAdapters = functionAdapters;
}
