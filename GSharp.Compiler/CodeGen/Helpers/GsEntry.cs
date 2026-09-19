namespace GSharp.Compiler.CodeGen.Helpers;

// One key/value pair of a G# map, as seen by a `for` loop over the map or by `map.entries`.
// Immutable, and equal to another entry when both its key and its value are equal.
public sealed class GsEntry(object key, object value)
{
    public object Key { get; } = key;
    public object Value { get; } = value;

    public override bool Equals(object? other)
    {
        return other is GsEntry entry && Equals(Key, entry.Key) && Equals(Value, entry.Value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Key, Value);
    }

    public override string ToString()
    {
        return RuntimeHelpers.FormatForDisplay(this);
    }
}
