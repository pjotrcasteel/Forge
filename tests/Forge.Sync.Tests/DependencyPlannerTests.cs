namespace Forge.Sync.Tests;

[TestClass]
public sealed class DependencyPlannerTests
{
    [TestMethod]
    public void Plan_ShouldCreatePrerequisiteFirstAndDeleteDependentFirst()
    {
        Node[] nodes =
        [
            new("internet", []),
            new("router", ["internet"]),
            new("port", ["router"])
        ];

        var plan = DependencyPlanner.Plan(nodes, static item => item.Id, static item => item.DependsOn);

        Assert.IsFalse(plan.HasCycles);
        Assert.AreEqual("internet", plan.CreateWaves[0].Items[0].Id);
        Assert.AreEqual("port", plan.DeleteWaves[0].Items[0].Id);
    }

    [TestMethod]
    public void Plan_WhenIndependent_ShouldPlaceItemsInSameWaveInInputOrder()
    {
        Node[] nodes = [new("a", []), new("b", [])];

        var plan = DependencyPlanner.Plan(nodes, static item => item.Id, static item => item.DependsOn);

        CollectionAssert.AreEqual(new[] { "a", "b" }, plan.CreateWaves[0].Items.Select(static item => item.Id).ToArray());
    }

    [TestMethod]
    public void Plan_WhenCycleExists_ShouldExposeCycleKeys()
    {
        Node[] nodes = [new("a", ["b"]), new("b", ["a"])];

        var plan = DependencyPlanner.Plan(nodes, static item => item.Id, static item => item.DependsOn);

        Assert.IsTrue(plan.HasCycles);
        CollectionAssert.AreEqual(new[] { "a", "b" }, plan.CycleKeys.ToArray());
    }

    private sealed record Node(string Id, IReadOnlyList<string> DependsOn);
}
