namespace Forge.Decide;

/// <summary>
/// Creates built-in deterministic strategy-selection policies.
/// </summary>
public static class StrategySelectionPolicies
{
    /// <summary>
    /// Creates the safe default policy that requires exactly one applicable strategy.
    /// </summary>
    /// <typeparam name="TContext">Decision context type.</typeparam>
    /// <typeparam name="TPlan">Application-owned plan type.</typeparam>
    /// <returns>An exactly-one selection policy.</returns>
    public static IStrategySelectionPolicy<TContext, TPlan> ExactlyOne<TContext, TPlan>() => new ExactlyOneStrategySelectionPolicy<TContext, TPlan>();
}