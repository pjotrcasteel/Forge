namespace Forge.Sync;

/// <summary>Immutable snapshot of which plan items are runnable and which remain blocked.</summary>
public sealed class ReadinessPlan<TItem, TKey, TReason>
    where TKey : notnull
    where TReason : notnull
{
    /// <summary>Creates a readiness snapshot.</summary>
    public ReadinessPlan(
        IReadOnlyList<ReadyPlanItem<TItem, TKey>> runnable,
        IReadOnlyList<BlockedPlanItem<TItem, TKey, TReason>> blocked)
    {
        ArgumentNullException.ThrowIfNull(runnable);
        ArgumentNullException.ThrowIfNull(blocked);
        Runnable = Array.AsReadOnly(runnable.ToArray());
        Blocked = Array.AsReadOnly(blocked.ToArray());
    }

    public IReadOnlyList<ReadyPlanItem<TItem, TKey>> Runnable { get; }
    public IReadOnlyList<BlockedPlanItem<TItem, TKey, TReason>> Blocked { get; }
    public bool AllRunnable => Blocked.Count == 0;
}
