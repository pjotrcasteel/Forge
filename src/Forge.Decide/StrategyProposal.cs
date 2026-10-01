namespace Forge.Decide;

/// <summary>
/// Represents the result of asking a strategy to formulate a plan.
/// </summary>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public abstract record StrategyProposal<TPlan>
{
    private protected StrategyProposal()
    {
    }

    /// <summary>
    /// Creates an applicable proposal.
    /// </summary>
    /// <param name="plan">Proposed application-owned plan.</param>
    /// <param name="reason">Optional explanation for the proposal.</param>
    /// <returns>An applicable proposal.</returns>
    public static StrategyProposal<TPlan> Applicable(TPlan plan, string? reason = null) => new ApplicableStrategyProposal<TPlan>(plan, reason);

    /// <summary>
    /// Creates an explicit rejection.
    /// </summary>
    /// <param name="reason">Explanation of why the strategy is not applicable.</param>
    /// <returns>An inapplicable proposal.</returns>
    public static StrategyProposal<TPlan> NotApplicable(string reason) => new InapplicableStrategyProposal<TPlan>(reason);
}