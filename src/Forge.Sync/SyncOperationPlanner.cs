using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Maps structural Sync changes to application-defined operation values without performing I/O.
/// </summary>
public static class SyncOperationPlanner
{
    public static SyncOperationPlan<T, TKey, TDelta, TOperation> Classify<T, TKey, TDelta, TOperation>(
        SyncPlan<T, TKey, TDelta> plan,
        Func<SyncAddition<T, TKey>, TOperation> additionSelector,
        Func<SyncUpdate<T, TKey, TDelta>, TOperation> updateSelector,
        Func<SyncRemoval<T, TKey>, TOperation> removalSelector)
        where TDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(additionSelector);
        ArgumentNullException.ThrowIfNull(updateSelector);
        ArgumentNullException.ThrowIfNull(removalSelector);

        var additions = plan.Added
            .Select(item => new PlannedAddition<T, TKey, TOperation>(
                item.Key,
                item.Desired,
                additionSelector(item)))
            .ToArray();
        var updates = plan.Updated
            .Select(item => new PlannedUpdate<T, TKey, TDelta, TOperation>(
                item.Key,
                item.Current,
                item.Desired,
                item.Delta,
                updateSelector(item)))
            .ToArray();
        var removals = plan.Removed
            .Select(item => new PlannedRemoval<T, TKey, TOperation>(
                item.Key,
                item.Current,
                removalSelector(item)))
            .ToArray();

        return new SyncOperationPlan<T, TKey, TDelta, TOperation>(additions, updates, removals);
    }
}
