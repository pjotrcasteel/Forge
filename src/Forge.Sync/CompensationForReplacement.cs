namespace Forge.Sync;

/// <summary>Represents completed work whose replacement requires compensation before the desired operation can safely proceed.</summary>
public sealed record CompensationForReplacement<TOperationId, TOperation, TKey>(
    TrackedPlanOperation<TOperationId, TOperation, TKey> Previous,
    TrackedPlanOperation<TOperationId, TOperation, TKey> Desired)
    where TKey : notnull;
