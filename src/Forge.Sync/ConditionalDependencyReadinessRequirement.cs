namespace Forge.Sync;

/// <summary>Adapts one typed conditional dependency into the general readiness requirement model.</summary>
public sealed class ConditionalDependencyReadinessRequirement<TKey, TState, TReason>
    : IReadinessRequirement<StateSnapshot<TKey, TState>, TReason>
    where TKey : notnull
    where TReason : notnull
{
    private readonly Func<BlockedConditionalDependency<TKey, TState>, TReason> _reasonFactory;

    /// <summary>Creates a readiness requirement for one predecessor-state dependency.</summary>
    public ConditionalDependencyReadinessRequirement(
        ConditionalDependency<TKey, TState> dependency,
        Func<BlockedConditionalDependency<TKey, TState>, TReason> reasonFactory)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(reasonFactory);
        Dependency = dependency;
        _reasonFactory = reasonFactory;
    }

    public ConditionalDependency<TKey, TState> Dependency { get; }

    /// <inheritdoc />
    public ReadinessRequirementResult<TReason> Evaluate(StateSnapshot<TKey, TState> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (!context.TryGetState(Dependency.Predecessor, out var state))
        {
            var blocked = new BlockedConditionalDependency<TKey, TState>(
                Dependency,
                ConditionalDependencyBlockReason.MissingPredecessorState,
                ObservedState<TState>.Missing());
            return ReadinessRequirementResult<TReason>.Blocked(_reasonFactory(blocked));
        }

        if (Dependency.Condition.IsSatisfied(state))
        {
            return ReadinessRequirementResult<TReason>.Satisfied();
        }

        var unsatisfied = new BlockedConditionalDependency<TKey, TState>(
            Dependency,
            ConditionalDependencyBlockReason.StateConditionNotSatisfied,
            ObservedState<TState>.Present(state));
        return ReadinessRequirementResult<TReason>.Blocked(_reasonFactory(unsatisfied));
    }
}
