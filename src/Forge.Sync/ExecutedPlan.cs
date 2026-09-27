namespace Forge.Sync;

/// <summary>Immutable typed operation plan tracked across execution-aware replanning cycles.</summary>
public sealed class ExecutedPlan<TOperationId, TOperation, TKey>
    where TKey : notnull
{
    /// <summary>Creates an immutable executed-plan snapshot.</summary>
    public ExecutedPlan(IReadOnlyList<TrackedPlanOperation<TOperationId, TOperation, TKey>> operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        Operations = Array.AsReadOnly(operations.ToArray());
    }

    public IReadOnlyList<TrackedPlanOperation<TOperationId, TOperation, TKey>> Operations { get; }
}
