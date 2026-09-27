namespace Forge.Sync;

/// <summary>Represents an item blocked by one or more typed readiness reasons.</summary>
public sealed class BlockedPlanItem<TItem, TKey, TReason>
    where TKey : notnull
    where TReason : notnull
{
    /// <summary>Creates an immutable blocked item.</summary>
    public BlockedPlanItem(TKey key, TItem item, IReadOnlyList<TReason> reasons)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(reasons);
        Key = key;
        Item = item;
        Reasons = Array.AsReadOnly(reasons.ToArray());
    }

    public TKey Key { get; }
    public TItem Item { get; }
    public IReadOnlyList<TReason> Reasons { get; }
}
