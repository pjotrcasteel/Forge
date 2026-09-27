namespace Forge.Sync;

/// <summary>Represents running work that the new desired state no longer requires.</summary>
public sealed record RunningRemovalConflict<TOperationId, TOperation, TKey>(
    TrackedPlanOperation<TOperationId, TOperation, TKey> Previous)
    where TKey : notnull;
