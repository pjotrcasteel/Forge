namespace Forge.Sync.Tests;

[TestClass]
public sealed class SyncPlanInverterTests
{
    [TestMethod]
    public void Invert_ShouldTurnForwardPlanIntoCompensationPlan()
    {
        OrderItem[] current = [new("a", "router", 1), new("remove", "old", 1)];
        OrderItem[] desired = [new("a", "router", 2), new("add", "new", 1)];
        var forward = OrderItemSync.Plan(current, desired);

        var reverse = SyncPlanInverter.Invert(forward, static delta => delta.Invert());

        Assert.AreEqual("remove", reverse.Added[0].Key.Id);
        Assert.AreEqual("add", reverse.Removed[0].Key.Id);
        Assert.AreEqual(2, reverse.Updated[0].Current.Quantity);
        Assert.AreEqual(1, reverse.Updated[0].Desired.Quantity);
    }

    [TestMethod]
    public void Invert_WhenPlanContainsPreservedItems_ShouldRejectAmbiguousReverse()
    {
        OrderItem[] current = [new("a", "router", 1)];
        OrderItem[] desired = [];
        var upsert = OrderItemSync.Plan(current, desired, SyncMode.Upsert);

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            SyncPlanInverter.Invert(upsert, static delta => delta.Invert()));
    }
}
