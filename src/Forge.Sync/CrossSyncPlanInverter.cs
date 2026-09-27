using Forge.Delta;

namespace Forge.Sync;

/// <summary>Reverses complete cross-type plans by swapping current/desired sides and inverting update Deltas.</summary>
public static class CrossSyncPlanInverter
{
    public static CrossSyncPlan<TDesired, TCurrent, TKey, TDelta> Invert<TCurrent, TDesired, TKey, TDelta>(
        CrossSyncPlan<TCurrent, TDesired, TKey, TDelta> plan,
        Func<TDelta, TDelta> deltaInverter)
        where TDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(deltaInverter);
        if (plan.Preserved.Count != 0)
        {
            throw new InvalidOperationException("Cross-type Upsert plans with preserved state cannot be inverted safely.");
        }

        return new CrossSyncPlan<TDesired, TCurrent, TKey, TDelta>(
            plan.Removed.Select(item => new CrossSyncAddition<TCurrent, TKey>(item.Key, item.Current)).ToArray(),
            plan.Added.Select(item => new CrossSyncRemoval<TDesired, TKey>(item.Key, item.Desired)).ToArray(),
            plan.Updated.Select(item => new CrossSyncUpdate<TDesired, TCurrent, TKey, TDelta>(
                item.Key,
                item.Desired,
                item.Current,
                deltaInverter(item.Delta))).ToArray(),
            plan.Unchanged.Select(item => new CrossSyncUnchanged<TDesired, TCurrent, TKey>(
                item.Key,
                item.Desired,
                item.Current)).ToArray(),
            Array.Empty<CrossSyncPreserved<TDesired, TKey>>());
    }
}
