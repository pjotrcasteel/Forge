namespace Forge.Sync;

/// <summary>
/// Represents a set of items whose in-plan dependencies are already satisfied and can execute in parallel.
/// </summary>
public sealed class DependencyWave<T>
{
    public DependencyWave(int index, IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Index = index;
        Items = Array.AsReadOnly(items.ToArray());
    }

    public int Index { get; }
    public IReadOnlyList<T> Items { get; }
}
