namespace Forge.Sync;

/// <summary>
/// Satisfies a dependency when the observed state equals a required state.
/// </summary>
public sealed class StateEqualsCondition<TState> : IStateCondition<TState>
{
    private readonly IEqualityComparer<TState> _comparer;

    /// <summary>Creates an equality-based state condition.</summary>
    public StateEqualsCondition(TState requiredState, IEqualityComparer<TState>? comparer = null)
    {
        RequiredState = requiredState;
        _comparer = comparer ?? EqualityComparer<TState>.Default;
    }

    /// <summary>Gets the required state.</summary>
    public TState RequiredState { get; }

    /// <inheritdoc />
    public bool IsSatisfied(TState state) => _comparer.Equals(state, RequiredState);
}
