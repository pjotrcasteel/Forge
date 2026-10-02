namespace Forge.Decide;

/// <summary>
/// Describes how one strategy's evidence changed across two evaluations.
/// </summary>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed record StrategyCandidateDiff<TPlan>
{
    internal StrategyCandidateDiff(
        StrategyId strategyId,
        StrategyCandidate<TPlan>? before,
        StrategyCandidate<TPlan>? after,
        StrategyCandidateChange changes)
    {
        StrategyId = strategyId;
        Before = before;
        After = after;
        Changes = changes;
    }

    /// <summary>
    /// Gets the strategy identifier.
    /// </summary>
    public StrategyId StrategyId { get; }

    /// <summary>
    /// Gets the earlier candidate evidence when present.
    /// </summary>
    public StrategyCandidate<TPlan>? Before { get; }

    /// <summary>
    /// Gets the later candidate evidence when present.
    /// </summary>
    public StrategyCandidate<TPlan>? After { get; }

    /// <summary>
    /// Gets the detected evidence changes.
    /// </summary>
    public StrategyCandidateChange Changes { get; }

    /// <summary>
    /// Gets whether any evidence changed.
    /// </summary>
    public bool HasChanges => Changes != StrategyCandidateChange.None;
}