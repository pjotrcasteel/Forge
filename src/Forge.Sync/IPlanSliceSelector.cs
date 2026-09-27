namespace Forge.Sync;

/// <summary>Selects plan items for a dependency-safe slice.</summary>
public interface IPlanSliceSelector<in TItem>
{
    /// <summary>Returns whether the item is directly requested by the slice.</summary>
    bool Include(TItem item);
}
