using Forge.Delta;

namespace Forge.Sync;

/// <summary>Creates compensation plans by reversing a complete replace-mode reconciliation.</summary>
public static class SyncPlanInverter
{
    public static SyncPlan<T, TKey, TDelta> Invert<T, TKey, TDelta>(
        SyncPlan<T, TKey, TDelta> plan,
        Func<TDelta, TDelta> deltaInverter)
        where TDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(deltaInverter);
        if (plan.Preserved.Count != 0)
        {
            throw new InvalidOperationException(
                "Upsert plans containing preserved current-only state cannot be inverted because the omitted desired state is unknown.");
        }

        var added = plan.Removed
            .Select(item => new SyncAddition<T, TKey>(item.Key, item.Current))
            .ToArray();
        var removed = plan.Added
            .Select(item => new SyncRemoval<T, TKey>(item.Key, item.Desired))
            .ToArray();
        var updated = plan.Updated
            .Select(item => new SyncUpdate<T, TKey, TDelta>(
                item.Key,
                item.Desired,
                item.Current,
                deltaInverter(item.Delta)))
            .ToArray();
        var unchanged = plan.Unchanged
            .Select(item => new SyncUnchanged<T, TKey>(item.Key, item.Desired, item.Current))
            .ToArray();

        return new SyncPlan<T, TKey, TDelta>(
            added,
            removed,
            updated,
            unchanged,
            Array.Empty<SyncPreserved<T, TKey>>());
    }
}
