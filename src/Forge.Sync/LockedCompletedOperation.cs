namespace Forge.Sync;

/// <summary>Represents completed work whose semantic operation remains valid and therefore stays locked.</summary>
public sealed record LockedCompletedOperation<TOperationId, TOperation, TKey>(
    TrackedPlanOperation<TOperationId, TOperation, TKey> Current)
    where TKey : notnull;
