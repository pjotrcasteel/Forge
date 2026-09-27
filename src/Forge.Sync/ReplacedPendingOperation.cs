namespace Forge.Sync;

/// <summary>Represents not-yet-started work replaced by a semantically different operation.</summary>
public sealed record ReplacedPendingOperation<TOperationId, TOperation, TKey>(
    TrackedPlanOperation<TOperationId, TOperation, TKey> Previous,
    TrackedPlanOperation<TOperationId, TOperation, TKey> Current)
    where TKey : notnull;
