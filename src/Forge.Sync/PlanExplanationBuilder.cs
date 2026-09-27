namespace Forge.Sync;

/// <summary>
/// Adds application-owned dependency, prerequisite, policy or execution facts to a structural plan explanation.
/// </summary>
public sealed class PlanExplanationBuilder<TKey>
    where TKey : notnull
{
    private readonly List<PlanExplanationEntry<TKey>> _entries;
    private readonly IEqualityComparer<TKey> _comparer;

    /// <summary>
    /// Starts from a structural explanation produced by Forge.
    /// </summary>
    public PlanExplanationBuilder(
        SyncPlanExplanation<TKey> explanation,
        IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(explanation);
        _comparer = comparer ?? EqualityComparer<TKey>.Default;
        _entries = explanation.Entries
            .Select(entry => new PlanExplanationEntry<TKey>(entry.Key, entry.Action, entry.Reasons.ToArray()))
            .ToList();
    }

    /// <summary>
    /// Adds an application-owned reason to an existing logical item.
    /// </summary>
    public PlanExplanationBuilder<TKey> AddReason(TKey key, PlanExplanationNode reason)
    {
        ArgumentNullException.ThrowIfNull(reason);
        var index = _entries.FindIndex(entry => _comparer.Equals(entry.Key, key));
        if (index < 0)
        {
            throw new KeyNotFoundException("The logical key does not exist in this plan explanation.");
        }

        var current = _entries[index];
        _entries[index] = current with { Reasons = Array.AsReadOnly(current.Reasons.Append(reason).ToArray()) };
        return this;
    }

    /// <summary>
    /// Builds an immutable explanation snapshot.
    /// </summary>
    public SyncPlanExplanation<TKey> Build()
        => new(_entries, _comparer);
}
