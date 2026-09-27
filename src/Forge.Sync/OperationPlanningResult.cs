namespace Forge.Sync;

/// <summary>
/// Combines typed application operations with the structured explanation of how they were selected.
/// </summary>
public sealed record OperationPlanningResult<TOperationPlan, TKey>(
    TOperationPlan Operations,
    SyncPlanExplanation<TKey> Explanation)
    where TKey : notnull;
