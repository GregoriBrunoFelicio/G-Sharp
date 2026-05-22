namespace GSharp.CodeGen.Helpers;

// Runtime representation of a G# function used as a first-class value.
//
// In G#, functions can be passed as arguments, stored in bindings, and
// returned from other functions. At the IL level, static methods cannot
// be passed around as plain objects — they need to be wrapped in a delegate.
//
// GSharpFunction wraps a Func<object[], object> delegate, which is itself
// created from an adapter method that unpacks an object[] into the individual
// parameters of the underlying static method.
//
// Direct calls (soma(3 5)) still use the fast Call opcode.
// Higher-order calls (apply(f 5)) go through GSharpFunction.Call.
public sealed class GSharpFunction
{
    private readonly Func<object[], object> _invoke;

    public GSharpFunction(Func<object[], object> invoke) => _invoke = invoke;

    public object Call(object[] args) => _invoke(args);

    public override string ToString() => "<function>";
}
