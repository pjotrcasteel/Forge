using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Describes application operations classified from a structural Sync plan without executing them.
/// </summary>
public sealed class SyncOperationPlan<T, TKey, TDelta, TOperation>
    where TDelta : IDelta
{
    public SyncOperationPlan(
        IReadOnlyList<PlannedAddition<T, TKey, TOperation>> additions,
        IReadOnlyList<PlannedUpdate<T, TKey, TDelta, TOperation>> updates,
        IReadOnlyList<PlannedRemoval<T, TKey, TOperation>> removals)
    {
        ArgumentNullException.ThrowIfNull(additions);
        ArgumentNullException.ThrowIfNull(updates);
        ArgumentNullException.ThrowIfNull(removals);
        Additions = Array.AsReadOnly(additions.ToArray());
        Updates = Array.AsReadOnly(updates.ToArray());
        Removals = Array.AsReadOnly(removals.ToArray());
    }

    public IReadOnlyList<PlannedAddition<T, TKey, TOperation>> Additions { get; }
    public IReadOnlyList<PlannedUpdate<T, TKey, TDelta, TOperation>> Updates { get; }
    public IReadOnlyList<PlannedRemoval<T, TKey, TOperation>> Removals { get; }
    public int Count => Additions.Count + Updates.Count + Removals.Count;
    public bool HasOperations => Count != 0;
}
