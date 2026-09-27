using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Entry point for strongly typed Sync constraints.
/// </summary>
public static class SyncPlanConstraints
{
    /// <summary>
    /// Creates a same-type Sync constraint builder.
    /// </summary>
    public static SyncPlanConstraintBuilder<T, TKey, TDelta> For<T, TKey, TDelta>()
        where TKey : notnull
        where TDelta : IDelta
        => new();

    /// <summary>
    /// Creates a generic dependency-plan constraint that rejects cycles.
    /// </summary>
    public static PlanConstraintSet<DependencyPlan<T, TKey>> RequireAcyclic<T, TKey>()
        => new PlanConstraintSet<DependencyPlan<T, TKey>>()
            .Require(
                "dependency.cycle",
                "The dependency plan must be acyclic before execution.",
                plan => !plan.HasCycles);
}
