namespace Forge.Sync;

/// <summary>Represents running work whose semantics remain valid in the new plan.</summary>
public sealed record PreservedRunningOperation<TOperationId, TOperation, TKey>(
    TrackedPlanOperation<TOperationId, TOperation, TKey> Current)
    where TKey : notnull;
