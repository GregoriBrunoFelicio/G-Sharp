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
// For arity-1 functions a Func<object, object> fast path is available via
// Call1, which avoids allocating an object[] on every higher-order call.
//
// Direct calls (soma(3 5)) still use the fast Call opcode.
// Higher-order calls (apply(f 5)) go through GSharpFunction.Call / Call1.
public sealed class GSharpFunction
{
    private readonly Func<object[], object>? _invokeN;
    private readonly Func<object, object>?  _invoke1;

    public GSharpFunction(Func<object[], object> invokeN) => _invokeN = invokeN;
    public GSharpFunction(Func<object, object>  invoke1) => _invoke1 = invoke1;

    public object Call(object[] args) =>
        _invoke1 is not null ? _invoke1(args[0]) : _invokeN!(args);

    // Single-argument fast path — avoids heap-allocating object[] for each call.
    public object Call1(object arg) =>
        _invoke1 is not null ? _invoke1(arg) : _invokeN!([arg]);

    public override string ToString() => "<function>";
}
