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

    /// <summary>
    /// Creates a policy that selects the applicable proposal with the highest application-defined value.
    /// Ties remain ambiguous and are never broken by registration order.
    /// </summary>
    /// <typeparam name="TContext">Decision context type.</typeparam>
    /// <typeparam name="TPlan">Application-owned plan type.</typeparam>
    /// <typeparam name="TValue">Comparable application-defined selection value.</typeparam>
    /// <param name="valueSelector">Function that calculates a value for each applicable proposal.</param>
    /// <param name="comparer">Optional comparer for the calculated values.</param>
    /// <returns>A highest-value selection policy.</returns>
    public static IStrategySelectionPolicy<TContext, TPlan> HighestBy<TContext, TPlan, TValue>(
        Func<TContext, ApplicableStrategyCandidate<TPlan>, TValue> valueSelector,
        IComparer<TValue>? comparer = null) =>
        new ExtremumStrategySelectionPolicy<TContext, TPlan, TValue>(valueSelector, comparer, selectHighest: true);

    /// <summary>
    /// Creates a policy that selects the applicable proposal with the lowest application-defined value.
    /// Ties remain ambiguous and are never broken by registration order.
    /// </summary>
    /// <typeparam name="TContext">Decision context type.</typeparam>
    /// <typeparam name="TPlan">Application-owned plan type.</typeparam>
    /// <typeparam name="TValue">Comparable application-defined selection value.</typeparam>
    /// <param name="valueSelector">Function that calculates a value for each applicable proposal.</param>
    /// <param name="comparer">Optional comparer for the calculated values.</param>
    /// <returns>A lowest-value selection policy.</returns>
    public static IStrategySelectionPolicy<TContext, TPlan> LowestBy<TContext, TPlan, TValue>(
        Func<TContext, ApplicableStrategyCandidate<TPlan>, TValue> valueSelector,
        IComparer<TValue>? comparer = null) =>
        new ExtremumStrategySelectionPolicy<TContext, TPlan, TValue>(valueSelector, comparer, selectHighest: false);
}