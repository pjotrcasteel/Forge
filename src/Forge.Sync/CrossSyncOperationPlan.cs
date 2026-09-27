using Forge.Delta;

namespace Forge.Sync;

/// <summary>Typed application operations classified from a cross-type Sync plan.</summary>
public sealed class CrossSyncOperationPlan<TCurrent, TDesired, TKey, TDelta, TOperation>
    where TDelta : IDelta
{
    public CrossSyncOperationPlan(
        IReadOnlyList<CrossPlannedAddition<TDesired, TKey, TOperation>> additions,
        IReadOnlyList<CrossPlannedUpdate<TCurrent, TDesired, TKey, TDelta, TOperation>> updates,
        IReadOnlyList<CrossPlannedRemoval<TCurrent, TKey, TOperation>> removals)
    {
        Additions = Array.AsReadOnly(additions.ToArray());
        Updates = Array.AsReadOnly(updates.ToArray());
        Removals = Array.AsReadOnly(removals.ToArray());
    }

    public IReadOnlyList<CrossPlannedAddition<TDesired, TKey, TOperation>> Additions { get; }
    public IReadOnlyList<CrossPlannedUpdate<TCurrent, TDesired, TKey, TDelta, TOperation>> Updates { get; }
    public IReadOnlyList<CrossPlannedRemoval<TCurrent, TKey, TOperation>> Removals { get; }
    public int Count => Additions.Count + Updates.Count + Removals.Count;
}
