namespace Forge.Sync;

/// <summary>
/// Describes the shortest discovered impact path from an original seed to one affected logical item.
/// </summary>
public sealed class ImpactPath<TKey, TReason>
{
    /// <summary>
    /// Creates an immutable impact path.
    /// </summary>
    public ImpactPath(TKey seed, TKey target, IReadOnlyList<ImpactEdge<TKey, TReason>> edges)
    {
        ArgumentNullException.ThrowIfNull(edges);
        Seed = seed;
        Target = target;
        Edges = Array.AsReadOnly(edges.ToArray());
    }

    public TKey Seed { get; }
    public TKey Target { get; }
    public IReadOnlyList<ImpactEdge<TKey, TReason>> Edges { get; }
    public int Distance => Edges.Count;
}
