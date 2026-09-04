namespace GSharp.Compiler.CodeGen.Helpers;

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
//
// Partial application: G# functions are curried, so calling with fewer
// arguments than Arity must yield a new function value rather than invoking
// the underlying delegate short-handed. Call/Call1 concatenate the newly
// supplied arguments onto whatever was already captured by a previous
// partial call, and only invoke the delegate once the total reaches Arity.
public sealed class GSharpFunction
{
    private readonly Func<object, object>? _invoke1;
    private readonly Func<object[], object>? _invokeN;
    private readonly object[] _captured;

    public int Arity { get; }

    public GSharpFunction(Func<object[], object> invokeN, int arity)
        : this(null, invokeN, arity, [])
    {
    }

    public GSharpFunction(Func<object, object> invoke1)
        : this(invoke1, null, 1, [])
    {
    }

    private GSharpFunction(
        Func<object, object>? invoke1, Func<object[], object>? invokeN, int arity, object[] captured)
    {
        _invoke1 = invoke1;
        _invokeN = invokeN;
        Arity = arity;
        _captured = captured;
    }

    public object Call(object[] args)
    {
        var combined = _captured.Length == 0 ? args : [.._captured, ..args];

        if (combined.Length < Arity)
            return new GSharpFunction(_invoke1, _invokeN, Arity, combined);
        if (combined.Length > Arity)
            throw new Exception($"function expects {Arity} argument(s), got {combined.Length}");

        return _invoke1 is not null ? _invoke1(combined[0]) : _invokeN!(combined);
    }

    // Single-argument fast path — avoids heap-allocating object[] for each call
    // when the function is already fully saturated by this one argument.
    public object Call1(object arg)
    {
        if (_captured.Length == 0 && Arity == 1)
            return _invoke1 is not null ? _invoke1(arg) : _invokeN!([arg]);

        return Call([arg]);
    }

    public override string ToString()
    {
        return "<function>";
    }
}