using Forge.Decide;

namespace Forge.Decide.Testing;

/// <summary>
/// Contains the evaluated evidence and resolution state of one strategy scenario.
/// </summary>
/// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed record StrategyScenarioResult<TSpace, TPlan>
{
    internal StrategyScenarioResult(
        string name,
        StrategyScenarioStatus status,
        StrategyComparison<TSpace, TPlan> comparison,
        StrategyDecision<TSpace, TPlan>? decision,
        IReadOnlyList<StrategyId> ambiguousStrategyIds)
    {
        Name = name;
        Status = status;
        Comparison = comparison;
        Decision = decision;
        AmbiguousStrategyIds = ambiguousStrategyIds;
    }

    /// <summary>
    /// Gets the scenario name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the scenario resolution state.
    /// </summary>
    public StrategyScenarioStatus Status { get; }

    /// <summary>
    /// Gets the complete candidate evidence evaluated for the scenario.
    /// </summary>
    public StrategyComparison<TSpace, TPlan> Comparison { get; }

    /// <summary>
    /// Gets the resolved decision when the scenario selected successfully.
    /// </summary>
    public StrategyDecision<TSpace, TPlan>? Decision { get; }

    /// <summary>
    /// Gets the strategies involved when the scenario is ambiguous.
    /// </summary>
    public IReadOnlyList<StrategyId> AmbiguousStrategyIds { get; }

    /// <summary>
    /// Gets the selected strategy identifier when the scenario resolved successfully.
    /// </summary>
    public StrategyId? SelectedStrategyId => Decision?.SelectedStrategyId;
}