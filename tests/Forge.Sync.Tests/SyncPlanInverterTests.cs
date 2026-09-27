namespace Forge.Sync.Tests;

[TestClass]
public sealed class SyncPlanInverterTests
{
    [TestMethod]
    public void Invert_ShouldTurnForwardPlanIntoCompensationPlan()
    {
        var existingId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var removedId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var addedId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        OrderItem[] current = [new(existingId, "router", 1), new(removedId, "old", 1)];
        OrderItem[] desired = [new(existingId, "router", 2), new(addedId, "new", 1)];
        var forward = OrderItemSync.Plan(current, desired);

        var reverse = SyncPlanInverter.Invert(forward, static delta => delta.Invert());

        Assert.AreEqual(removedId, reverse.Added[0].Key.Id);
        Assert.AreEqual(addedId, reverse.Removed[0].Key.Id);
        Assert.AreEqual(2, reverse.Updated[0].Current.Quantity);
        Assert.AreEqual(1, reverse.Updated[0].Desired.Quantity);
    }

    [TestMethod]
    public void Invert_WhenPlanContainsPreservedItems_ShouldRejectAmbiguousReverse()
    {
        var existingId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        OrderItem[] current = [new(existingId, "router", 1)];
        OrderItem[] desired = [];
        var upsert = OrderItemSync.Plan(current, desired, SyncMode.Upsert);

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            SyncPlanInverter.Invert(upsert, static delta => delta.Invert()));
    }
}
