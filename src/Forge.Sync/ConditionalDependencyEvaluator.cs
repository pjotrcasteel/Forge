namespace Forge.Sync;

/// <summary>Evaluates typed conditional dependencies without executing or mutating operations.</summary>
public static class ConditionalDependencyEvaluator
{
    /// <summary>Evaluates dependencies against an immutable typed state snapshot.</summary>
    public static ConditionalDependencyEvaluation<TKey, TState> Evaluate<TKey, TState>(
        IReadOnlyList<ConditionalDependency<TKey, TState>> dependencies,
        StateSnapshot<TKey, TState> snapshot)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(snapshot);
        var satisfied = new List<ConditionalDependency<TKey, TState>>(dependencies.Count);
        var blocked = new List<BlockedConditionalDependency<TKey, TState>>();

        foreach (var dependency in dependencies)
        {
            ArgumentNullException.ThrowIfNull(dependency);
            if (!snapshot.TryGetState(dependency.Predecessor, out var state))
            {
                blocked.Add(new BlockedConditionalDependency<TKey, TState>(
                    dependency,
                    ConditionalDependencyBlockReason.MissingPredecessorState,
                    ObservedState<TState>.Missing()));
                continue;
            }

            if (!dependency.Condition.IsSatisfied(state))
            {
                blocked.Add(new BlockedConditionalDependency<TKey, TState>(
                    dependency,
                    ConditionalDependencyBlockReason.StateConditionNotSatisfied,
                    ObservedState<TState>.Present(state)));
                continue;
            }

            satisfied.Add(dependency);
        }

        return new ConditionalDependencyEvaluation<TKey, TState>(satisfied, blocked);
    }
}
