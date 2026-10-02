namespace Forge.Decide;

/// <summary>
/// Selects one candidate from the set of applicable proposals.
/// </summary>
/// <typeparam name="TContext">Decision context type.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public interface IStrategySelectionPolicy<in TContext, TPlan>
{
    /// <summary>
    /// Selects one of the supplied applicable candidates.
    /// </summary>
    /// <param name="context">Decision context.</param>
    /// <param name="candidates">Applicable candidates only.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The selected candidate.</returns>
    ValueTask<ApplicableStrategyCandidate<TPlan>> SelectAsync(
        TContext context,
        IReadOnlyList<ApplicableStrategyCandidate<TPlan>> candidates,
        CancellationToken cancellationToken);
}