namespace Forge.Decide;

/// <summary>
/// Compares completed decisions across two evaluations of the same strategy space.
/// </summary>
public static class StrategyDecisionDiff
{
    /// <summary>
    /// Compares selected strategies and their complete candidate evidence.
    /// </summary>
    /// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
    /// <typeparam name="TPlan">Application-owned plan type.</typeparam>
    /// <param name="before">Earlier decision.</param>
    /// <param name="after">Later decision.</param>
    /// <param name="canonicalizePlan">Application-owned deterministic canonicalizer for proposed plans.</param>
    /// <returns>A decision diff.</returns>
    public static StrategyDecisionDiff<TSpace, TPlan> Between<TSpace, TPlan>(
        StrategyDecision<TSpace, TPlan> before,
        StrategyDecision<TSpace, TPlan> after,
        Func<TPlan, string> canonicalizePlan)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);

        var beforeComparison = new StrategyComparison<TSpace, TPlan>(before.SpaceId, before.Candidates);
        var afterComparison = new StrategyComparison<TSpace, TPlan>(after.SpaceId, after.Candidates);
        var evidence = StrategyComparisonDiff.Between(beforeComparison, afterComparison, canonicalizePlan);

        return new StrategyDecisionDiff<TSpace, TPlan>(before.SelectedStrategyId, after.SelectedStrategyId, evidence);
    }
}