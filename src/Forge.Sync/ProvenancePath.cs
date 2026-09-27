namespace Forge.Sync;

/// <summary>Represents one shortest typed root-cause path ending at a traced target.</summary>
public sealed class ProvenancePath<TNode, TKey>
    where TKey : notnull
{
    /// <summary>Creates an immutable provenance path.</summary>
    public ProvenancePath(IReadOnlyList<TNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        if (nodes.Count == 0)
        {
            throw new ArgumentException("A provenance path must contain at least one node.", nameof(nodes));
        }

        Nodes = Array.AsReadOnly(nodes.ToArray());
    }

    public IReadOnlyList<TNode> Nodes { get; }
}
