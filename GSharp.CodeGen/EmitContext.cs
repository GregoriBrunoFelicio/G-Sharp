using System.Reflection.Emit;

namespace GSharp.CodeGen;

// Carries all the state an emitter needs to emit IL for a given scope.
//
// Before EmitContext existed, emitters received a raw Dictionary<string, LocalBuilder>
// and had no way to access function definitions or distinguish between local variables
// and function parameters. EmitContext bundles all of that together in one place.
//
// Each function body gets its own EmitContext so that its locals and parameters
// are completely separate from Main's locals and from other functions.
public class EmitContext(Dictionary<string, MethodBuilder> functions)
{
    // Maps variable names declared with 'let' to their IL local variable slots.
    // When the emitter encounters a VariableExpression, it looks here first.
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

    // Maps function names to their MethodBuilder.
    // Populated in pass 1 of the compiler before any IL is emitted,
    // so that forward calls and recursive calls can be resolved.
    //
    // Example: 'soma(10 20)' emits Call(functions["soma"]).
    public readonly Dictionary<string, MethodBuilder> Functions = functions;
}
