namespace Forge.Sync;

/// <summary>Immutable provenance trace for one target node.</summary>
public sealed class ProvenanceTrace<TNode, TKey>
    where TKey : notnull
{
    /// <summary>Creates a trace from the affected nodes and shortest root paths.</summary>
    public ProvenanceTrace(TNode target, IReadOnlyList<TNode> affectedBy, IReadOnlyList<ProvenancePath<TNode, TKey>> rootPaths)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(affectedBy);
        ArgumentNullException.ThrowIfNull(rootPaths);
        Target = target;
        AffectedBy = Array.AsReadOnly(affectedBy.ToArray());
        RootPaths = Array.AsReadOnly(rootPaths.ToArray());
    }

    public TNode Target { get; }
    public IReadOnlyList<TNode> AffectedBy { get; }
    public IReadOnlyList<ProvenancePath<TNode, TKey>> RootPaths { get; }
}
