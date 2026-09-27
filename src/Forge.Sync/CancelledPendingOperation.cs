namespace Forge.Sync;

/// <summary>Represents not-yet-started work that is no longer required by the replanned desired state.</summary>
public sealed record CancelledPendingOperation<TOperationId, TOperation, TKey>(
    TrackedPlanOperation<TOperationId, TOperation, TKey> Previous)
    where TKey : notnull;
