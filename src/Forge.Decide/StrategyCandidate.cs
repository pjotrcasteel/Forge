namespace Forge.Decide;

/// <summary>
/// Represents the evaluated result of one strategy in a strategy space.
/// </summary>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public abstract record StrategyCandidate<TPlan>
{
    private protected StrategyCandidate(StrategyId strategyId)
    {
        StrategyId = strategyId;
    }

    /// <summary>
    /// Gets the strategy identifier.
    /// </summary>
    public StrategyId StrategyId { get; }

    /// <summary>
    /// Gets whether this candidate is applicable.
    /// </summary>
    public abstract bool IsApplicable { get; }
}