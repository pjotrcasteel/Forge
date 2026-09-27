using Forge.Delta;

namespace Forge.Sync;

/// <summary>Classifies cross-type structural changes into application-defined operations.</summary>
public static class CrossSyncOperationPlanner
{
    public static CrossSyncOperationPlan<TCurrent, TDesired, TKey, TDelta, TOperation>
        Classify<TCurrent, TDesired, TKey, TDelta, TOperation>(
            CrossSyncPlan<TCurrent, TDesired, TKey, TDelta> plan,
            Func<CrossSyncAddition<TDesired, TKey>, TOperation> additionSelector,
            Func<CrossSyncUpdate<TCurrent, TDesired, TKey, TDelta>, TOperation> updateSelector,
            Func<CrossSyncRemoval<TCurrent, TKey>, TOperation> removalSelector)
        where TDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(additionSelector);
        ArgumentNullException.ThrowIfNull(updateSelector);
        ArgumentNullException.ThrowIfNull(removalSelector);
        return new CrossSyncOperationPlan<TCurrent, TDesired, TKey, TDelta, TOperation>(
            plan.Added.Select(item => new CrossPlannedAddition<TDesired, TKey, TOperation>(
                item.Key,
                item.Desired,
                additionSelector(item))).ToArray(),
            plan.Updated.Select(item => new CrossPlannedUpdate<TCurrent, TDesired, TKey, TDelta, TOperation>(
                item.Key,
                item.Current,
                item.Desired,
                item.Delta,
                updateSelector(item))).ToArray(),
            plan.Removed.Select(item => new CrossPlannedRemoval<TCurrent, TKey, TOperation>(
                item.Key,
                item.Current,
                removalSelector(item))).ToArray());
    }
}
