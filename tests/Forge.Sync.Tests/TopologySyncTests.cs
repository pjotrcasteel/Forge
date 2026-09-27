using Forge.Delta;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class TopologySyncTests
{
    [TestMethod]
    public void Plan_ShouldReconcileNodesAndEdgesTogether()
    {
        Node[] currentNodes = [new("access", "old"), new("router", "x")];
        Node[] desiredNodes = [new("access", "new"), new("firewall", "y")];
        Edge[] currentEdges = [new("e1", "router", "access")];
        Edge[] desiredEdges = [new("e2", "firewall", "access")];

        var plan = TopologySync.Plan(
            currentNodes,
            desiredNodes,
            currentEdges,
            desiredEdges,
            NodeDefinition(),
            EdgeDefinition());

        Assert.AreEqual(1, plan.Nodes.Added.Count);
        Assert.AreEqual(1, plan.Nodes.Removed.Count);
        Assert.AreEqual(1, plan.Nodes.Updated.Count);
        Assert.AreEqual(1, plan.Edges.Added.Count);
        Assert.AreEqual(1, plan.Edges.Removed.Count);
        Assert.AreEqual(TopologyPhase.RemoveEdges, plan.Phases[0]);
    }

    [TestMethod]
    public void Plan_WhenEdgeReferencesMissingNode_ShouldFailBeforeReconciliation()
    {
        Node[] nodes = [new("access", "x")];
        Edge[] edges = [new("broken", "missing", "access")];

        Assert.ThrowsExactly<InvalidTopologyException>(() => TopologySync.Plan(
            nodes,
            nodes,
            edges,
            edges,
            NodeDefinition(),
            EdgeDefinition()));
    }

    private static TopologyNodeDefinition<Node, string, TestDelta> NodeDefinition()
        => new(
            static node => node.Id,
            static (left, right) => left.Value == right.Value,
            static (left, right) => new TestDelta(left.Value, right.Value));

    private static TopologyEdgeDefinition<Edge, string, string, TestDelta> EdgeDefinition()
        => new(
            static edge => edge.Id,
            static edge => edge.Source,
            static edge => edge.Target,
            static (left, right) => left == right,
            static (left, right) => new TestDelta(left.ToString()!, right.ToString()!));

    private sealed record Node(string Id, string Value);
    private sealed record Edge(string Id, string Source, string Target);

    private sealed class TestDelta : IDelta
    {
        public TestDelta(string before, string after)
        {
            Changes = before == after ? [] : [new PropertyChange("Value", before, after)];
        }

        public bool HasChanges => Changes.Count != 0;
        public IReadOnlyList<PropertyChange> Changes { get; }
    }
}
