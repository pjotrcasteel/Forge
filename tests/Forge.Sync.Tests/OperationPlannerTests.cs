namespace Forge.Sync.Tests;

[TestClass]
public sealed class OperationPlannerTests
{
    [TestMethod]
    public void Classify_ShouldAllowDeltaAwareOperationSelection()
    {
        OrderItem[] current = [new("a", "router", 1)];
        OrderItem[] desired = [new("a", "firewall", 1), new("b", "access", 1)];
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
