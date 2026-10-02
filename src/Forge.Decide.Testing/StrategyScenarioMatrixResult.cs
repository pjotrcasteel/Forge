namespace Forge.Decide.Testing;

/// <summary>
/// Contains deterministic results for an evaluated strategy scenario matrix.
/// </summary>
/// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed record StrategyScenarioMatrixResult<TSpace, TPlan>
{
    internal StrategyScenarioMatrixResult(IReadOnlyList<StrategyScenarioResult<TSpace, TPlan>> scenarios)
    {
        Scenarios = scenarios;
    }

    /// <summary>
    /// Gets scenario results in input order.
    /// </summary>
    public IReadOnlyList<StrategyScenarioResult<TSpace, TPlan>> Scenarios { get; }

    /// <summary>
    /// Gets scenarios for which no strategy was applicable.
    /// </summary>
    public IReadOnlyList<StrategyScenarioResult<TSpace, TPlan>> Uncovered =>
        Scenarios.Where(scenario => scenario.Status == StrategyScenarioStatus.NoApplicableStrategy).ToArray();

    /// <summary>
    /// Gets scenarios whose configured selection policy was ambiguous.
    /// </summary>
    public IReadOnlyList<StrategyScenarioResult<TSpace, TPlan>> Ambiguous =>
        Scenarios.Where(scenario => scenario.Status == StrategyScenarioStatus.Ambiguous).ToArray();

    /// <summary>
    /// Gets whether every scenario resolves to one strategy decision.
    /// </summary>
    public bool IsFullyResolved => Scenarios.All(scenario => scenario.Status == StrategyScenarioStatus.Selected);
}