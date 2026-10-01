namespace Forge.Decide;

/// <summary>
/// Produces a side-effect-free proposal for a context.
/// </summary>
/// <typeparam name="TContext">Context used to formulate the proposal.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public interface IStrategy<in TContext, TPlan>
{
    /// <summary>
    /// Gets the stable strategy identifier.
    /// </summary>
    StrategyId Id { get; }

    /// <summary>
    /// Formulates a proposal without executing the resulting plan.
    /// </summary>
    /// <param name="context">Decision context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An applicable proposal or an explicit rejection.</returns>
    ValueTask<StrategyProposal<TPlan>> ProposeAsync(TContext context, CancellationToken cancellationToken);
}