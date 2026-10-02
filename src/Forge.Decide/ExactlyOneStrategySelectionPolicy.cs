namespace Forge.Decide;

/// <summary>
/// Requires exactly one applicable strategy.
/// </summary>
/// <typeparam name="TContext">Decision context type.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed class ExactlyOneStrategySelectionPolicy<TContext, TPlan> : IStrategySelectionPolicy<TContext, TPlan>
{
    /// <inheritdoc />
    public ValueTask<ApplicableStrategyCandidate<TPlan>> SelectAsync(
        TContext context,
        IReadOnlyList<ApplicableStrategyCandidate<TPlan>> candidates,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        cancellationToken.ThrowIfCancellationRequested();

        if (candidates.Count != 1)
        {
            throw new AmbiguousStrategyDecisionException(candidates.Select(candidate => candidate.StrategyId).ToArray());
        }

        return ValueTask.FromResult(candidates[0]);
    }
}