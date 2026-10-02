namespace Forge.Decide;

/// <summary>
/// Represents changes across two completed strategy decisions.
/// </summary>
/// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed record StrategyDecisionDiff<TSpace, TPlan>
{
    internal StrategyDecisionDiff(
        StrategyId beforeSelectedStrategyId,
        StrategyId afterSelectedStrategyId,
        StrategyComparisonDiff<TSpace, TPlan> evidence)
    {
        BeforeSelectedStrategyId = beforeSelectedStrategyId;
        AfterSelectedStrategyId = afterSelectedStrategyId;
        Evidence = evidence;
    }

    /// <summary>
    /// Gets the earlier selected strategy identifier.
    /// </summary>
    public StrategyId BeforeSelectedStrategyId { get; }

    /// <summary>
    /// Gets the later selected strategy identifier.
    /// </summary>
    public StrategyId AfterSelectedStrategyId { get; }

    /// <summary>
    /// Gets whether strategy selection changed.
    /// </summary>
    public bool SelectionChanged => BeforeSelectedStrategyId != AfterSelectedStrategyId;

    /// <summary>
    /// Gets the complete candidate evidence diff.
    /// </summary>
    public StrategyComparisonDiff<TSpace, TPlan> Evidence { get; }

    /// <summary>
    /// Gets whether selection or candidate evidence changed.
    /// </summary>
    public bool HasChanges => SelectionChanged || Evidence.HasChanges;
}