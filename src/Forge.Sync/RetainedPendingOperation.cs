namespace Forge.Sync;

/// <summary>Represents not-yet-started work retained with its existing operation identity.</summary>
public sealed record RetainedPendingOperation<TOperationId, TOperation, TKey>(
    TrackedPlanOperation<TOperationId, TOperation, TKey> Current)
    where TKey : notnull;
