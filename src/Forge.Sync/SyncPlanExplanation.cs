namespace Forge.Sync;

/// <summary>
/// Immutable, inspectable explanation of a Sync plan.
/// </summary>
public sealed class SyncPlanExplanation<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Creates a plan explanation and snapshots all entries.
    /// </summary>
    public SyncPlanExplanation(
        IReadOnlyList<PlanExplanationEntry<TKey>> entries,
        IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Entries = Array.AsReadOnly(entries.ToArray());
        var effectiveComparer = comparer ?? EqualityComparer<TKey>.Default;
        var byKey = new Dictionary<TKey, PlanExplanationEntry<TKey>>(effectiveComparer);
        foreach (var entry in Entries)
        {
            if (!byKey.TryAdd(entry.Key, entry))
            {
                throw new ArgumentException("Plan explanations require unique logical keys.", nameof(entries));
            }
        }

        ByKey = new System.Collections.ObjectModel.ReadOnlyDictionary<TKey, PlanExplanationEntry<TKey>>(byKey);
    }

    /// <summary>Gets explanation entries in deterministic plan order.</summary>
    public IReadOnlyList<PlanExplanationEntry<TKey>> Entries { get; }

    /// <summary>Gets explanation entries indexed by logical key.</summary>
    public IReadOnlyDictionary<TKey, PlanExplanationEntry<TKey>> ByKey { get; }
}
