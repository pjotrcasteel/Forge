using Forge.Sync;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class ImpactAnalyzerTests
{
    [TestMethod]
    public void Analyze_ShouldSeparateDirectAndTransitiveImpactAndRetainShortestPath()
    {
        var edges = new[]
        {
            new ImpactEdge<string, string>("subnet", "ipvpn", "framedIpv4"),
            new ImpactEdge<string, string>("ipvpn", "internet", "service-state"),
            new ImpactEdge<string, string>("subnet", "inventory", "resource-link")
        };

        var result = ImpactAnalyzer.Analyze(["subnet"], edges, StringComparer.OrdinalIgnoreCase);

        CollectionAssert.AreEqual(new[] { "ipvpn", "inventory" }, result.DirectlyAffected.ToArray());
        CollectionAssert.AreEqual(new[] { "internet" }, result.TransitivelyAffected.ToArray());
        Assert.AreEqual(2, result.Paths["internet"].Distance);
        Assert.AreEqual("framedIpv4", result.Paths["internet"].Edges[0].Reason);
    }

    [TestMethod]
    public void Analyze_WhenGraphContainsCycle_ShouldNotRevisitSeed()
    {
        var edges = new[]
        {
            new ImpactEdge<string, string>("A", "B", "one"),
            new ImpactEdge<string, string>("B", "C", "two"),
            new ImpactEdge<string, string>("C", "A", "cycle")
        };

        var result = ImpactAnalyzer.Analyze(["A"], edges);

        Assert.AreEqual(2, result.AffectedCount);
        Assert.IsFalse(result.Paths.ContainsKey("A"));
    }
}
