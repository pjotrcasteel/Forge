namespace Forge.Decide;

/// <summary>
/// Thrown when the configured policy cannot resolve multiple applicable strategies.
/// </summary>
public sealed class AmbiguousStrategyDecisionException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception.
    /// </summary>
    /// <param name="strategyIds">Applicable strategy identifiers.</param>
    public AmbiguousStrategyDecisionException(IReadOnlyList<StrategyId> strategyIds)
        : base($"Strategy selection is ambiguous between: {string.Join(", ", strategyIds)}.")
    {
        StrategyIds = strategyIds;
    }

    /// <summary>
    /// Gets the applicable strategy identifiers.
    /// </summary>
    public IReadOnlyList<StrategyId> StrategyIds { get; }
}