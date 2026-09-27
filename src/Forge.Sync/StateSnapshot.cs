namespace Forge.Sync;

/// <summary>Immutable strongly typed state lookup used by readiness and dependency evaluation.</summary>
public sealed class StateSnapshot<TKey, TState>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TState> _states;

    /// <summary>Creates an immutable snapshot by copying the supplied states.</summary>
    public StateSnapshot(IReadOnlyDictionary<TKey, TState> states, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(states);
        _states = new Dictionary<TKey, TState>(states.Count, comparer ?? EqualityComparer<TKey>.Default);
        foreach (var pair in states)
        {
            if (!_states.TryAdd(pair.Key, pair.Value))
            {
                throw new ArgumentException("The state snapshot contains duplicate keys.", nameof(states));
            }
        }
    }

    /// <summary>Gets the number of observed states.</summary>
    public int Count => _states.Count;

    /// <summary>Attempts to read a state value.</summary>
    public bool TryGetState(TKey key, out TState state) => _states.TryGetValue(key, out state!);

    /// <summary>Gets a required state value.</summary>
    public TState GetState(TKey key)
        => _states.TryGetValue(key, out var state)
            ? state
            : throw new KeyNotFoundException("The requested state key is not present in the snapshot.");
}
