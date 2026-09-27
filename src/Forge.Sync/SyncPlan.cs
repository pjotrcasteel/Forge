using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Describes the deterministic reconciliation plan between current and desired collections.
/// </summary>
/// <typeparam name="T">The synchronized item type.</typeparam>
/// <typeparam name="TKey">The generated logical key type.</typeparam>
/// <typeparam name="TDelta">The generated delta type.</typeparam>
[System.Diagnostics.DebuggerDisplay(
    "Added = {Added.Count}, Removed = {Removed.Count}, Updated = {Updated.Count}, Preserved = {Preserved.Count}")]
public sealed class SyncPlan<T, TKey, TDelta>
    where TDelta : IDelta
{
    /// <summary>
    /// Initializes a sync plan and snapshots all supplied result collections.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public SyncPlan(
        IReadOnlyList<SyncAddition<T, TKey>> added,
        IReadOnlyList<SyncRemoval<T, TKey>> removed,
        IReadOnlyList<SyncUpdate<T, TKey, TDelta>> updated,
        IReadOnlyList<SyncUnchanged<T, TKey>> unchanged,
        IReadOnlyList<SyncPreserved<T, TKey>> preserved)
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

    /// <summary>
    /// Gets items present only in the supplied desired collection.
    /// </summary>
    public IReadOnlyList<SyncAddition<T, TKey>> Added { get; }

    /// <summary>
    /// Gets current-only items that should be removed in replace mode.
    /// </summary>
    public IReadOnlyList<SyncRemoval<T, TKey>> Removed { get; }

    /// <summary>
    /// Gets items present in both collections whose state changed.
    /// </summary>
    public IReadOnlyList<SyncUpdate<T, TKey, TDelta>> Updated { get; }

    /// <summary>
    /// Gets items present in both collections whose state did not change.
    /// </summary>
    public IReadOnlyList<SyncUnchanged<T, TKey>> Unchanged { get; }

    /// <summary>
    /// Gets current-only items intentionally left untouched in upsert mode.
    /// </summary>
    public IReadOnlyList<SyncPreserved<T, TKey>> Preserved { get; }

    /// <summary>
    /// Gets the number of add, remove and update operations required by this plan.
    /// </summary>
    public int ChangeCount => Added.Count + Removed.Count + Updated.Count;

    /// <summary>
    /// Gets a value indicating whether this plan requires any state-changing operation.
    /// </summary>
    public bool HasChanges => ChangeCount != 0;

    /// <summary>
    /// Gets the number of items from the current collection represented by this plan.
    /// </summary>
    public int CurrentCount => Removed.Count + Updated.Count + Unchanged.Count + Preserved.Count;

    /// <summary>
    /// Gets the number of items from the supplied desired collection represented by this plan.
    /// </summary>
    public int DesiredCount => Added.Count + Updated.Count + Unchanged.Count;
}
