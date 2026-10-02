namespace Forge.Decide.OpenTelemetry;

/// <summary>
/// Adds ActivitySource instrumentation to individual strategies and selection policies.
/// </summary>
public static class StrategyTelemetryExtensions
{
    /// <summary>
    /// Wraps a strategy with proposal-level tracing.
    /// </summary>
    public static IStrategy<TContext, TPlan> WithOpenTelemetry<TContext, TPlan>(this IStrategy<TContext, TPlan> strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        return new InstrumentedStrategy<TContext, TPlan>(strategy);
    }

    /// <summary>
    /// Wraps a selection policy with selection-level tracing.
    /// </summary>
    public static IStrategySelectionPolicy<TContext, TPlan> WithOpenTelemetry<TContext, TPlan>(
        this IStrategySelectionPolicy<TContext, TPlan> selectionPolicy)
    {
        ArgumentNullException.ThrowIfNull(selectionPolicy);
        return new InstrumentedSelectionPolicy<TContext, TPlan>(selectionPolicy);
    }
}