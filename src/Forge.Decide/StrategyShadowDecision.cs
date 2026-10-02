namespace Forge.Decide;

/// <summary>
/// Contains production and shadow selections made from the same evaluated candidate evidence.
/// </summary>
/// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed record StrategyShadowDecision<TSpace, TPlan>
{
    internal StrategyShadowDecision(StrategyDecision<TSpace, TPlan> production, StrategyDecision<TSpace, TPlan> shadow)
    {
        Production = production;
        Shadow = shadow;
    }

    /// <summary>
    /// Gets the production decision.
    /// </summary>
    public StrategyDecision<TSpace, TPlan> Production { get; }

    /// <summary>
    /// Gets the shadow decision.
    /// </summary>
    public StrategyDecision<TSpace, TPlan> Shadow { get; }

    /// <summary>
    /// Gets whether the shadow policy selects a different strategy.
    /// </summary>
    public bool SelectionChanged => Production.SelectedStrategyId != Shadow.SelectedStrategyId;
}