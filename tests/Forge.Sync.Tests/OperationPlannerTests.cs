namespace Forge.Sync.Tests;

[TestClass]
public sealed class OperationPlannerTests
{
    [TestMethod]
    public void Classify_ShouldAllowDeltaAwareOperationSelection()
    {
        var existingId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var addedId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        OrderItem[] current = [new(existingId, "router", 1)];
        OrderItem[] desired = [new(existingId, "firewall", 1), new(addedId, "access", 1)];
        var plan = OrderItemSync.Plan(current, desired);

        var operations = SyncOperationPlanner.Classify(
            plan,
            static _ => Operation.Create,
            static update => update.Delta.ProductChange.HasChanged ? Operation.Replace : Operation.Update,
            static _ => Operation.Delete);

        Assert.AreEqual(Operation.Replace, operations.Updates[0].Operation);
        Assert.AreEqual(Operation.Create, operations.Additions[0].Operation);
    }

    private enum Operation
    {
        Create,
        Update,
        Replace,
        Delete
    }
}
