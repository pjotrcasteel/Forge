namespace Forge.Sync;

/// <summary>One typed planned operation with an application-owned stable execution identity.</summary>
public sealed record TrackedPlanOperation<TOperationId, TOperation, TKey>(
    TOperationId OperationId,
    TKey Key,
    TOperation Operation)
    where TKey : notnull;
