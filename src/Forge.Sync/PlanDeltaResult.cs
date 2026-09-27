using Forge.Delta;

namespace Forge.Sync;

/// <summary>Strongly typed semantic difference between two complete plan snapshots.</summary>
public sealed class PlanDeltaResult<TOperation, TOperationKey, TOperationDelta, TDependency, TDependencyKey, TDependencyDelta>
    where TOperationKey : notnull
    where TDependencyKey : notnull
    where TOperationDelta : IDelta
    where TDependencyDelta : IDelta
{
    /// <summary>Creates a plan Delta result.</summary>
    public PlanDeltaResult(
        CrossSyncPlan<TOperation, TOperation, TOperationKey, TOperationDelta> operations,
        CrossSyncPlan<TDependency, TDependency, TDependencyKey, TDependencyDelta> dependencies)
    {
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(dependencies);
        Operations = operations;
        Dependencies = dependencies;
    }

    public CrossSyncPlan<TOperation, TOperation, TOperationKey, TOperationDelta> Operations { get; }
    public CrossSyncPlan<TDependency, TDependency, TDependencyKey, TDependencyDelta> Dependencies { get; }
    public bool HasChanges => Operations.HasChanges || Dependencies.HasChanges;
    public int ChangeCount => Operations.ChangeCount + Dependencies.ChangeCount;
}
