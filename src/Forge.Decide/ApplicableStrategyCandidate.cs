namespace Forge.Decide;

/// <summary>
/// Represents an applicable candidate and its proposed plan.
/// </summary>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed record ApplicableStrategyCandidate<TPlan> : StrategyCandidate<TPlan>
{
    internal ApplicableStrategyCandidate(StrategyId strategyId, TPlan plan, string? reason)
        : base(strategyId)
    {
        Plan = plan;
        Reason = reason;
    }

    /// <inheritdoc />
    public override bool IsApplicable => true;

    /// <summary>
    /// Gets the proposed plan.
    /// </summary>
    public TPlan Plan { get; }

    /// <summary>
    /// Gets the optional explanation for the proposal.
    /// </summary>
    public string? Reason { get; }
}