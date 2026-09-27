namespace Forge.Sync;

/// <summary>Represents completed work that the new desired state would remove and therefore may require compensation.</summary>
public sealed record CompensationForRemoval<TOperationId, TOperation, TKey>(
    TrackedPlanOperation<TOperationId, TOperation, TKey> Previous)
    where TKey : notnull;
