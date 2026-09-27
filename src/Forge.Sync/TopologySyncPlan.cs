using Forge.Delta;

namespace Forge.Sync;

/// <summary>Describes node and edge reconciliation as one validated topology transition.</summary>
public sealed class TopologySyncPlan<TNode, TNodeKey, TNodeDelta, TEdge, TEdgeKey, TEdgeDelta>
    where TNodeKey : notnull
    where TEdgeKey : notnull
    where TNodeDelta : IDelta
    where TEdgeDelta : IDelta
{
    public TopologySyncPlan(
        CrossSyncPlan<TNode, TNode, TNodeKey, TNodeDelta> nodes,
        CrossSyncPlan<TEdge, TEdge, TEdgeKey, TEdgeDelta> edges)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);
        Nodes = nodes;
        Edges = edges;
    }

    public CrossSyncPlan<TNode, TNode, TNodeKey, TNodeDelta> Nodes { get; }
    public CrossSyncPlan<TEdge, TEdge, TEdgeKey, TEdgeDelta> Edges { get; }
    public bool HasChanges => Nodes.HasChanges || Edges.HasChanges;
    public int ChangeCount => Nodes.ChangeCount + Edges.ChangeCount;

    /// <summary>Gets the graph-safe structural phase order for consumers that execute the transition.</summary>
    public IReadOnlyList<TopologyPhase> Phases { get; } =
    [
        TopologyPhase.RemoveEdges,
        TopologyPhase.RemoveNodes,
        TopologyPhase.AddNodes,
        TopologyPhase.UpdateNodes,
        TopologyPhase.UpdateEdges,
        TopologyPhase.AddEdges
    ];
}
