using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Produces structured explanations from same-type and cross-type reconciliation plans.
/// </summary>
public static class SyncPlanExplainer
{
    /// <summary>
    /// Explains a same-type Sync plan.
    /// </summary>
    public static SyncPlanExplanation<TKey> Explain<T, TKey, TDelta>(
        SyncPlan<T, TKey, TDelta> plan,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
        where TDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(plan);
        var entries = new List<PlanExplanationEntry<TKey>>(
            plan.Added.Count + plan.Updated.Count + plan.Removed.Count + plan.Unchanged.Count + plan.Preserved.Count);

        entries.AddRange(plan.Added.Select(item => Entry(item.Key, PlanExplanationAction.Added, "sync.added", "Desired state introduces this item.")));
        entries.AddRange(plan.Updated.Select(item => UpdatedEntry(item.Key, item.Delta)));
        entries.AddRange(plan.Removed.Select(item => Entry(item.Key, PlanExplanationAction.Removed, "sync.removed", "Desired state no longer contains this item.")));
        entries.AddRange(
            plan.Unchanged.Select(item => Entry(
                item.Key,
                PlanExplanationAction.Unchanged,
                "sync.unchanged",
                "Current and desired state are semantically equivalent.")));
        entries.AddRange(
            plan.Preserved.Select(item => Entry(
                item.Key,
                PlanExplanationAction.Preserved,
                "sync.preserved",
                "The item is absent from an Upsert payload and is intentionally preserved.")));

        return new SyncPlanExplanation<TKey>(entries, comparer);
    }

    /// <summary>
    /// Explains a cross-type Sync plan.
    /// </summary>
    public static SyncPlanExplanation<TKey> Explain<TCurrent, TDesired, TKey, TDelta>(
        CrossSyncPlan<TCurrent, TDesired, TKey, TDelta> plan,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
        where TDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(plan);
        var entries = new List<PlanExplanationEntry<TKey>>(
            plan.Added.Count + plan.Updated.Count + plan.Removed.Count + plan.Unchanged.Count + plan.Preserved.Count);

        entries.AddRange(plan.Added.Select(item => Entry(item.Key, PlanExplanationAction.Added, "sync.added", "Desired state introduces this item.")));
        entries.AddRange(plan.Updated.Select(item => UpdatedEntry(item.Key, item.Delta)));
        entries.AddRange(plan.Removed.Select(item => Entry(item.Key, PlanExplanationAction.Removed, "sync.removed", "Desired state no longer contains this item.")));
        entries.AddRange(
            plan.Unchanged.Select(item => Entry(
                item.Key,
                PlanExplanationAction.Unchanged,
                "sync.unchanged",
                "Current and desired state are semantically equivalent.")));
        entries.AddRange(
            plan.Preserved.Select(item => Entry(
                item.Key,
                PlanExplanationAction.Preserved,
                "sync.preserved",
                "The item is absent from an Upsert payload and is intentionally preserved.")));

        return new SyncPlanExplanation<TKey>(entries, comparer);
    }

    private static PlanExplanationEntry<TKey> UpdatedEntry<TKey, TDelta>(TKey key, TDelta delta)
        where TDelta : IDelta
    {
        var propertyReasons = delta.Changes
            .Select(change => new PlanExplanationNode(
                "delta.property.changed",
                $"{change.Path} changed.",
                change.Path,
                change.Before,
                change.After))
            .ToArray();

        var root = new PlanExplanationNode(
            "sync.updated",
            "Current and desired state differ semantically.",
            children: propertyReasons);
        return new PlanExplanationEntry<TKey>(key, PlanExplanationAction.Updated, [root]);
    }

    private static PlanExplanationEntry<TKey> Entry<TKey>(
        TKey key,
        PlanExplanationAction action,
        string code,
        string summary)
        => new(key, action, [new PlanExplanationNode(code, summary)]);
}
