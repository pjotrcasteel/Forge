namespace Forge.Sync;

/// <summary>Immutable result of evaluating typed conditional dependencies against a state snapshot.</summary>
public sealed class ConditionalDependencyEvaluation<TKey, TState>
    where TKey : notnull
{
    /// <summary>Creates a dependency evaluation result.</summary>
    public ConditionalDependencyEvaluation(
        IReadOnlyList<ConditionalDependency<TKey, TState>> satisfied,
        IReadOnlyList<BlockedConditionalDependency<TKey, TState>> blocked)
    {
        ArgumentNullException.ThrowIfNull(satisfied);
        ArgumentNullException.ThrowIfNull(blocked);
        Satisfied = Array.AsReadOnly(satisfied.ToArray());
        Blocked = Array.AsReadOnly(blocked.ToArray());
    }

    /// <summary>Gets currently satisfied dependencies in input order.</summary>
    public IReadOnlyList<ConditionalDependency<TKey, TState>> Satisfied { get; }

    /// <summary>Gets currently blocked dependencies in input order.</summary>
    public IReadOnlyList<BlockedConditionalDependency<TKey, TState>> Blocked { get; }

    /// <summary>Gets whether every supplied dependency is satisfied.</summary>
    public bool AllSatisfied => Blocked.Count == 0;
}
