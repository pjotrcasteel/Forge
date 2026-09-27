namespace Forge.Sync;

/// <summary>Immutable application execution-state snapshot keyed by stable operation identity.</summary>
public sealed class ExecutionSnapshot<TOperationId, TState>
    where TOperationId : notnull
{
    private readonly Dictionary<TOperationId, TState> _states;

    /// <summary>Creates a snapshot by copying the supplied states.</summary>
    public ExecutionSnapshot(
        IReadOnlyDictionary<TOperationId, TState> states,
        IEqualityComparer<TOperationId>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(states);
        _states = new Dictionary<TOperationId, TState>(states.Count, comparer ?? EqualityComparer<TOperationId>.Default);
        foreach (var pair in states)
        {
            if (!_states.TryAdd(pair.Key, pair.Value))
            {
                throw new ArgumentException("The execution snapshot contains duplicate operation identities.", nameof(states));
            }
        }
    }

    public bool TryGetState(TOperationId operationId, out TState state) => _states.TryGetValue(operationId, out state!);
}
