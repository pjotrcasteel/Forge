namespace Forge.Sync;

/// <summary>
/// Represents a typed operation dependency that becomes satisfied only when the predecessor reaches an accepted state.
/// </summary>
public sealed record ConditionalDependency<TKey, TState>
    where TKey : notnull
{
    /// <summary>Creates a conditional dependency.</summary>
    public ConditionalDependency(TKey predecessor, TKey successor, IStateCondition<TState> condition)
    {
        ArgumentNullException.ThrowIfNull(condition);
        Predecessor = predecessor;
        Successor = successor;
        Condition = condition;
    }

    /// <summary>Gets the predecessor operation key.</summary>
    public TKey Predecessor { get; }

    /// <summary>Gets the successor operation key.</summary>
    public TKey Successor { get; }

    /// <summary>Gets the strongly typed state condition.</summary>
    public IStateCondition<TState> Condition { get; }
}
