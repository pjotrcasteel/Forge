using Forge.Delta;

namespace Forge.Sync;

/// <summary>Validates and reconciles graph nodes and relationships as one desired-state topology transition.</summary>
public static class TopologySync
{
    public static TopologySyncPlan<TNode, TNodeKey, TNodeDelta, TEdge, TEdgeKey, TEdgeDelta>
        Plan<TNode, TNodeKey, TNodeDelta, TEdge, TEdgeKey, TEdgeDelta>(
            IReadOnlyList<TNode> currentNodes,
            IReadOnlyList<TNode> desiredNodes,
            IReadOnlyList<TEdge> currentEdges,
            IReadOnlyList<TEdge> desiredEdges,
            TopologyNodeDefinition<TNode, TNodeKey, TNodeDelta> nodeDefinition,
            TopologyEdgeDefinition<TEdge, TEdgeKey, TNodeKey, TEdgeDelta> edgeDefinition)
        where TNodeKey : notnull
        where TEdgeKey : notnull
        where TNodeDelta : IDelta
        where TEdgeDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(nodeDefinition);
        ArgumentNullException.ThrowIfNull(edgeDefinition);
        ValidateTopology(
            currentNodes,
            currentEdges,
            nodeDefinition,
            edgeDefinition,
            nameof(currentNodes));
        ValidateTopology(
            desiredNodes,
            desiredEdges,
            nodeDefinition,
            edgeDefinition,
            nameof(desiredNodes));

        var nodeSyncDefinition = new CrossSyncDefinition<TNode, TNode, TNodeKey, TNodeDelta>(
            nodeDefinition.KeySelector,
            nodeDefinition.KeySelector,
            nodeDefinition.AreEquivalent,
            nodeDefinition.DeltaFactory,
            SyncMode.Replace,
            nodeDefinition.KeyComparer);
        var edgeSyncDefinition = new CrossSyncDefinition<TEdge, TEdge, TEdgeKey, TEdgeDelta>(
            edgeDefinition.KeySelector,
            edgeDefinition.KeySelector,
            edgeDefinition.AreEquivalent,
            edgeDefinition.DeltaFactory,
            SyncMode.Replace,
            edgeDefinition.KeyComparer);
        var nodePlan = CrossSync.Plan(currentNodes, desiredNodes, nodeSyncDefinition);
        var edgePlan = CrossSync.Plan(currentEdges, desiredEdges, edgeSyncDefinition);

        return new TopologySyncPlan<TNode, TNodeKey, TNodeDelta, TEdge, TEdgeKey, TEdgeDelta>(nodePlan, edgePlan);
    }

    private static void ValidateTopology<TNode, TNodeKey, TNodeDelta, TEdge, TEdgeKey, TEdgeDelta>(
        IReadOnlyList<TNode> nodes,
        IReadOnlyList<TEdge> edges,
        TopologyNodeDefinition<TNode, TNodeKey, TNodeDelta> nodeDefinition,
        TopologyEdgeDefinition<TEdge, TEdgeKey, TNodeKey, TEdgeDelta> edgeDefinition,
        string collectionName)
        where TNodeKey : notnull
        where TEdgeKey : notnull
        where TNodeDelta : IDelta
        where TEdgeDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);
        var nodeKeys = new HashSet<TNodeKey>(nodeDefinition.KeyComparer);
        foreach (var node in nodes)
        {
            ArgumentNullException.ThrowIfNull(node);
            var key = nodeDefinition.KeySelector(node);
            if (!nodeKeys.Add(key))
            {
                throw new DuplicateSyncKeyException(typeof(TNode), collectionName, Convert.ToString(key) ?? string.Empty);
            }
        }

        foreach (var edge in edges)
        {
            ArgumentNullException.ThrowIfNull(edge);
            var source = edgeDefinition.SourceKeySelector(edge);
            var target = edgeDefinition.TargetKeySelector(edge);
            if (!nodeKeys.Contains(source))
            {
                throw new InvalidTopologyException(
                    collectionName,
                    Convert.ToString(edgeDefinition.KeySelector(edge)),
                    Convert.ToString(source));
            }

            if (!nodeKeys.Contains(target))
            {
                throw new InvalidTopologyException(
                    collectionName,
                    Convert.ToString(edgeDefinition.KeySelector(edge)),
                    Convert.ToString(target));
            }
        }
    }
}
