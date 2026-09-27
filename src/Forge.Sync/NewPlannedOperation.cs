namespace Forge.Sync;

/// <summary>Represents a newly required operation introduced by replanning.</summary>
public sealed record NewPlannedOperation<TOperationId, TOperation, TKey>(
    TrackedPlanOperation<TOperationId, TOperation, TKey> Current)
    where TKey : notnull;
