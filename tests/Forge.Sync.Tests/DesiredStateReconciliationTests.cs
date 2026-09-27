namespace Forge.Sync.Tests;

[TestClass]
public sealed class DesiredStateReconciliationTests
{
    [TestMethod]
    public void Plan_WhenDesiredTopologyChanges_ClassifiesEveryLogicalComponent()
    {
        IReadOnlyList<ManagedComponent> current =
        [
            Component("access", "Access", null, 100),
            Component("subnet", "Subnet", "10.1.0.0/24", 100),
            Component("router", "Router", null, 100)
        ];
        IReadOnlyList<ManagedComponent> desired =
        [
            Component("access", "Access", null, 100),
            Component("subnet", "Subnet", "10.2.0.0/24", 100),
            Component("firewall", "Firewall", null, 100)
        ];

        var plan = ManagedComponentSync.Plan(current, desired);

        Assert.AreEqual(1, plan.Added.Count);
        Assert.AreEqual("firewall", plan.Added[0].Key.LogicalId);
        Assert.AreEqual(1, plan.Removed.Count);
        Assert.AreEqual("router", plan.Removed[0].Key.LogicalId);
        Assert.AreEqual(1, plan.Updated.Count);
        Assert.AreEqual("subnet", plan.Updated[0].Key.LogicalId);
        Assert.AreEqual("Configuration.Network", plan.Updated[0].Delta.Changes.Single().Path);
        Assert.AreEqual(1, plan.Unchanged.Count);
        Assert.AreEqual("access", plan.Unchanged[0].Key.LogicalId);
    }

    [TestMethod]
    public void Plan_WhenDesiredStateIsEmpty_ProducesPureRemovalPlan()
    {
        IReadOnlyList<ManagedComponent> current =
        [
            Component("access", "Access", null, 100),
            Component("subnet", "Subnet", "10.1.0.0/24", 100)
        ];

        var plan = ManagedComponentSync.Plan(current, Array.Empty<ManagedComponent>());

        Assert.IsTrue(plan.HasChanges);
        Assert.AreEqual(0, plan.Added.Count);
        Assert.AreEqual(2, plan.Removed.Count);
        Assert.AreEqual(0, plan.Updated.Count);
        Assert.AreEqual(0, plan.Unchanged.Count);
    }

    [TestMethod]
    public void Plan_WhenDesiredStateIsReduced_ProducesOnlyRemovedAndUnchangedItems()
    {
        IReadOnlyList<ManagedComponent> current =
        [
            Component("access", "Access", null, 100),
            Component("router", "Router", null, 100),
            Component("subnet", "Subnet", "10.1.0.0/24", 100)
        ];
        IReadOnlyList<ManagedComponent> desired =
        [
            Component("access", "Access", null, 100),
            Component("subnet", "Subnet", "10.1.0.0/24", 100)
        ];

        var plan = ManagedComponentSync.Plan(current, desired);

        Assert.AreEqual(1, plan.Removed.Count);
        Assert.AreEqual("router", plan.Removed[0].Key.LogicalId);
        Assert.AreEqual(2, plan.Unchanged.Count);
        Assert.AreEqual(0, plan.Added.Count);
        Assert.AreEqual(0, plan.Updated.Count);
    }

    private static ManagedComponent Component(
        string logicalId,
        string kind,
        string? network,
        int capacity)
    {
        return new ManagedComponent(
            logicalId,
            kind,
            new ManagedComponentConfiguration(network, capacity));
    }
}
