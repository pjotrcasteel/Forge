using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Describes reconciliation between current and desired collections whose item CLR types differ.
/// </summary>
public sealed class CrossSyncPlan<TCurrent, TDesired, TKey, TDelta>
    where TDelta : IDelta
{
    /// <summary>
    /// Creates an immutable cross-type reconciliation plan.
    /// </summary>
    public CrossSyncPlan(
        IReadOnlyList<CrossSyncAddition<TDesired, TKey>> added,
        IReadOnlyList<CrossSyncRemoval<TCurrent, TKey>> removed,
        IReadOnlyList<CrossSyncUpdate<TCurrent, TDesired, TKey, TDelta>> updated,
        IReadOnlyList<CrossSyncUnchanged<TCurrent, TDesired, TKey>> unchanged,
        IReadOnlyList<CrossSyncPreserved<TCurrent, TKey>> preserved)
    {
        ArgumentNullException.ThrowIfNull(added);
        ArgumentNullException.ThrowIfNull(removed);
        ArgumentNullException.ThrowIfNull(updated);
        ArgumentNullException.ThrowIfNull(unchanged);
        ArgumentNullException.ThrowIfNull(preserved);
        Added = Array.AsReadOnly(added.ToArray());
        Removed = Array.AsReadOnly(removed.ToArray());
        Updated = Array.AsReadOnly(updated.ToArray());
        Unchanged = Array.AsReadOnly(unchanged.ToArray());
        Preserved = Array.AsReadOnly(preserved.ToArray());
    }

    public IReadOnlyList<CrossSyncAddition<TDesired, TKey>> Added { get; }
    public IReadOnlyList<CrossSyncRemoval<TCurrent, TKey>> Removed { get; }
    public IReadOnlyList<CrossSyncUpdate<TCurrent, TDesired, TKey, TDelta>> Updated { get; }
    public IReadOnlyList<CrossSyncUnchanged<TCurrent, TDesired, TKey>> Unchanged { get; }
    public IReadOnlyList<CrossSyncPreserved<TCurrent, TKey>> Preserved { get; }
    public int ChangeCount => Added.Count + Removed.Count + Updated.Count;
    public bool HasChanges => ChangeCount != 0;
}
