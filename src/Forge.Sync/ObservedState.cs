namespace Forge.Sync;

/// <summary>
/// Represents whether a state value is present without using null as a sentinel.
/// </summary>
public readonly struct ObservedState<TState>
{
    private readonly TState? _value;

    private ObservedState(bool hasValue, TState? value)
    {
        HasValue = hasValue;
        _value = value;
    }

    /// <summary>Gets whether a state value is present.</summary>
    public bool HasValue { get; }

    /// <summary>Gets the observed state.</summary>
    public TState Value => HasValue
        ? _value!
        : throw new InvalidOperationException("No state value is present.");

    /// <summary>Creates a present state observation.</summary>
    public static ObservedState<TState> Present(TState value) => new(true, value);

    /// <summary>Creates a missing state observation.</summary>
    public static ObservedState<TState> Missing() => new(false, default);
}
