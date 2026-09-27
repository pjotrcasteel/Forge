namespace Forge.Delta;

/// <summary>
/// Describes the before and after values for one strongly typed property.
/// </summary>
/// <typeparam name="T">The property type.</typeparam>
[System.Diagnostics.DebuggerDisplay("{Before} -> {After} (Changed = {HasChanged})")]
public readonly record struct ValueChange<T>
{
    private ValueChange(T before, T after, bool hasChanged)
    {
        Before = before;
        After = after;
        HasChanged = hasChanged;
    }

    /// <summary>
    /// Gets the value before the comparison.
    /// </summary>
    public T Before { get; }

    /// <summary>
    /// Gets the value after the comparison.
    /// </summary>
    public T After { get; }

    /// <summary>
    /// Gets a value indicating whether the property changed.
    /// </summary>
    public bool HasChanged { get; }

    /// <summary>
    /// Creates a change using the default equality comparer for the property type.
    /// </summary>
    public static ValueChange<T> Create(T before, T after)
    {
        return Create(before, after, EqualityComparer<T>.Default);
    }

    /// <summary>
    /// Creates a change using the supplied equality comparer.
    /// </summary>
    public static ValueChange<T> Create(T before, T after, IEqualityComparer<T> comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        return new ValueChange<T>(before, after, !comparer.Equals(before, after));
    }

    /// <summary>
    /// Creates a change from a comparison result that has already been determined.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static ValueChange<T> FromComparison(T before, T after, bool hasChanged)
    {
        return new ValueChange<T>(before, after, hasChanged);
    }
}
