namespace Forge.Sync;

/// <summary>Immutable dependency-safe subset of a plan.</summary>
public sealed class PlanSlice<TItem, TKey>
    where TKey : notnull
{
    /// <summary>Creates a plan slice.</summary>
    public PlanSlice(
        IReadOnlyList<TItem> included,
        IReadOnlyList<TItem> directlySelected,
        IReadOnlyList<TItem> requiredDependencies,
        IReadOnlyList<TItem> excluded)
    {
        ArgumentNullException.ThrowIfNull(included);
        ArgumentNullException.ThrowIfNull(directlySelected);
        ArgumentNullException.ThrowIfNull(requiredDependencies);
        ArgumentNullException.ThrowIfNull(excluded);
        Included = Array.AsReadOnly(included.ToArray());
        DirectlySelected = Array.AsReadOnly(directlySelected.ToArray());
        RequiredDependencies = Array.AsReadOnly(requiredDependencies.ToArray());
        Excluded = Array.AsReadOnly(excluded.ToArray());
    }

    public IReadOnlyList<TItem> Included { get; }
    public IReadOnlyList<TItem> DirectlySelected { get; }
    public IReadOnlyList<TItem> RequiredDependencies { get; }
    public IReadOnlyList<TItem> Excluded { get; }
}
