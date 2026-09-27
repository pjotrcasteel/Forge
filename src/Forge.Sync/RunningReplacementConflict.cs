namespace Forge.Sync;

/// <summary>Represents running work whose semantics conflict with a newly desired replacement.</summary>
public sealed record RunningReplacementConflict<TOperationId, TOperation, TKey>(
    TrackedPlanOperation<TOperationId, TOperation, TKey> Previous,
    TrackedPlanOperation<TOperationId, TOperation, TKey> Desired)
    where TKey : notnull;
