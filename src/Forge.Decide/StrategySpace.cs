namespace Forge.Decide;

/// <summary>
/// Defines explicit candidate boundaries for a consumer-defined strategy-space marker.
/// </summary>
/// <typeparam name="TSpace">Consumer-defined marker type that distinguishes this strategy space at compile time.</typeparam>
public static class StrategySpace<TSpace>
{
    /// <summary>
    /// Starts defining a typed strategy space.
    /// </summary>
    /// <typeparam name="TContext">Decision context type.</typeparam>
    /// <typeparam name="TPlan">Application-owned plan type.</typeparam>
    /// <param name="id">Stable strategy-space identifier used for diagnostics and persisted evidence.</param>
    /// <returns>A strategy-space builder.</returns>
    public static StrategySpaceBuilder<TSpace, TContext, TPlan> Define<TContext, TPlan>(StrategySpaceId id) => new(id);
}