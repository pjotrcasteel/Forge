namespace Forge.Decide;

/// <summary>
/// Represents a candidate that rejected the supplied context.
/// </summary>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed record RejectedStrategyCandidate<TPlan> : StrategyCandidate<TPlan>
{
    internal RejectedStrategyCandidate(StrategyId strategyId, string reason)
        : base(strategyId)
    {
        Reason = reason;
    }

    /// <inheritdoc />
    public override bool IsApplicable => false;

    /// <summary>
    /// Gets the rejection explanation.
    /// </summary>
    public string Reason { get; }
}