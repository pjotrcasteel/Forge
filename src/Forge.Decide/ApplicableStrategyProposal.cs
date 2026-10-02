namespace Forge.Decide;

/// <summary>
/// Represents an applicable strategy proposal.
/// </summary>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed record ApplicableStrategyProposal<TPlan> : StrategyProposal<TPlan>
{
    /// <summary>
    /// Initializes an applicable proposal.
    /// </summary>
    /// <param name="plan">Proposed plan.</param>
    /// <param name="reason">Optional explanation for the proposal.</param>
    public ApplicableStrategyProposal(TPlan plan, string? reason = null)
    {
        Plan = plan;
        Reason = reason;
    }

    /// <summary>
    /// Gets the proposed plan.
    /// </summary>
    public TPlan Plan { get; }

    /// <summary>
    /// Gets the optional explanation for the proposal.
    /// </summary>
    public string? Reason { get; }
}