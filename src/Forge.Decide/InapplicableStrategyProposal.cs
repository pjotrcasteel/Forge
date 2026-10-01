namespace Forge.Decide;

/// <summary>
/// Represents a strategy that is not applicable to the supplied context.
/// </summary>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed record InapplicableStrategyProposal<TPlan> : StrategyProposal<TPlan>
{
    /// <summary>
    /// Initializes an inapplicable proposal.
    /// </summary>
    /// <param name="reason">Explanation of why the strategy is not applicable.</param>
    public InapplicableStrategyProposal(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Reason = reason;
    }

    /// <summary>
    /// Gets the explanation of why the strategy is not applicable.
    /// </summary>
    public string Reason { get; }
}