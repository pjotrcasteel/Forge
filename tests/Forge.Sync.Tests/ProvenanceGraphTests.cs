namespace Forge.Sync.Tests;

[TestClass]
public sealed class ProvenanceGraphTests
{
    [TestMethod]
    public void Trace_ShouldReturnTypedAncestorsAndShortestRootPaths()
    {
        CauseNode[] nodes =
        [
            new(new CauseId(1), CauseKind.CustomerChange),
            new(new CauseId(2), CauseKind.Qualification),
            new(new CauseId(3), CauseKind.DerivedFact),
            new(new CauseId(4), CauseKind.Operation)
        ];
        ProvenanceEdge<CauseId>[] edges =
        [
            new(new CauseId(1), new CauseId(3)),
            new(new CauseId(2), new CauseId(3)),
            new(new CauseId(3), new CauseId(4))
        ];
        var graph = ProvenanceGraph<CauseNode, CauseId>.Create(
            nodes,
            edges,
            new ProvenanceDefinition<CauseNode, CauseId>(static node => node.Id));

        var trace = graph.Trace(new CauseId(4));

        Assert.AreEqual(CauseKind.Operation, trace.Target.Kind);
        Assert.AreEqual(3, trace.AffectedBy.Count);
        Assert.AreEqual(2, trace.RootPaths.Count);
        CollectionAssert.AreEqual(
            new[] { CauseKind.CustomerChange, CauseKind.DerivedFact, CauseKind.Operation },
            trace.RootPaths[0].Nodes.Select(static node => node.Kind).ToArray());
    }

    [TestMethod]
    public void Create_WhenEdgeReferencesUnknownNode_ShouldFailBeforeTracing()
    {
        CauseNode[] nodes = [new(new CauseId(1), CauseKind.CustomerChange)];
        ProvenanceEdge<CauseId>[] edges = [new(new CauseId(1), new CauseId(2))];

        Assert.ThrowsExactly<ArgumentException>(() => ProvenanceGraph<CauseNode, CauseId>.Create(
            nodes,
            edges,
            new ProvenanceDefinition<CauseNode, CauseId>(static node => node.Id)));
    }


    [TestMethod]
    public void Create_WhenGraphContainsCycle_ShouldFailBeforeTracing()
    {
        CauseNode[] nodes =
        [
            new(new CauseId(1), CauseKind.CustomerChange),
            new(new CauseId(2), CauseKind.DerivedFact)
        ];
        ProvenanceEdge<CauseId>[] edges =
        [
            new(new CauseId(1), new CauseId(2)),
            new(new CauseId(2), new CauseId(1))
        ];

        Assert.ThrowsExactly<ArgumentException>(() => ProvenanceGraph<CauseNode, CauseId>.Create(
            nodes,
            edges,
            new ProvenanceDefinition<CauseNode, CauseId>(static node => node.Id)));
    }

    [TestMethod]
    public void Trace_WhenComparerTreatsDuplicateEdgesAsEqual_ShouldIncludeCauseOnlyOnce()
    {
        StringCauseNode[] nodes =
        [
            new("source", CauseKind.CustomerChange),
            new("target", CauseKind.Operation)
        ];
        ProvenanceEdge<string>[] edges =
        [
            new("source", "target"),
            new("SOURCE", "TARGET")
        ];
        var graph = ProvenanceGraph<StringCauseNode, string>.Create(
            nodes,
            edges,
            new ProvenanceDefinition<StringCauseNode, string>(static node => node.Id, StringComparer.OrdinalIgnoreCase));

        var trace = graph.Trace("TARGET");

        Assert.AreEqual(1, trace.AffectedBy.Count);
        Assert.AreEqual(1, trace.RootPaths.Count);
    }

    private sealed record CauseNode(CauseId Id, CauseKind Kind);
    private sealed record StringCauseNode(string Id, CauseKind Kind);
    private readonly record struct CauseId(int Value);

    private enum CauseKind
    {
        CustomerChange,
        Qualification,
        DerivedFact,
        Operation
    }
}
