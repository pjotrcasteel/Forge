namespace Forge.Decide;

/// <summary>
/// Thrown when a custom selection policy returns a candidate outside the current strategy space evaluation.
/// </summary>
public sealed class InvalidStrategySelectionException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception.
    /// </summary>
    /// <param name="strategyId">Invalid selected strategy identifier.</param>
    public InvalidStrategySelectionException(StrategyId strategyId)
        : base($"Selection policy returned strategy '{strategyId}', which is not one of the current applicable candidates.")
    {
        StrategyId = strategyId;
    }

    /// <summary>
    /// Gets the invalid strategy identifier.
    /// </summary>
    public StrategyId StrategyId { get; }
}