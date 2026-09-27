namespace Forge.Sync;

/// <summary>
/// Immutable result of graph impact propagation from one or more changed logical items.
/// </summary>
public sealed class ImpactAnalysis<TKey, TReason>
    where TKey : notnull
{
    /// <summary>
    /// Creates an impact result.
    /// </summary>
    public ImpactAnalysis(
        IReadOnlyList<TKey> seeds,
        IReadOnlyList<TKey> directlyAffected,
        IReadOnlyList<TKey> transitivelyAffected,
        IReadOnlyDictionary<TKey, ImpactPath<TKey, TReason>> paths)
    {
        ArgumentNullException.ThrowIfNull(seeds);
        ArgumentNullException.ThrowIfNull(directlyAffected);
        ArgumentNullException.ThrowIfNull(transitivelyAffected);
        ArgumentNullException.ThrowIfNull(paths);
        Seeds = Array.AsReadOnly(seeds.ToArray());
        DirectlyAffected = Array.AsReadOnly(directlyAffected.ToArray());
        TransitivelyAffected = Array.AsReadOnly(transitivelyAffected.ToArray());
        Paths = paths;
    }

    public IReadOnlyList<TKey> Seeds { get; }
    public IReadOnlyList<TKey> DirectlyAffected { get; }
    public IReadOnlyList<TKey> TransitivelyAffected { get; }
    public IReadOnlyDictionary<TKey, ImpactPath<TKey, TReason>> Paths { get; }
    public int AffectedCount => DirectlyAffected.Count + TransitivelyAffected.Count;
}
