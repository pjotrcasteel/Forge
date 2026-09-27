using Forge.Sync;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class OperationRulesTests
{
    private enum Operation
    {
        Add,
        Modify,
        Replace,
        Delete
    }

    [TestMethod]
    public void Plan_WhenSpecificDeltaRuleMatches_ShouldSelectRuleAndExplainIt()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var current = new[] { new OrderItem(id, "router", 1) };
        var desired = new[] { new OrderItem(id, "firewall", 1) };
        var structural = OrderItemSync.Plan(current, desired);
        var rules = new SyncOperationRules<OrderItem, OrderItemSync.Key, OrderItemDelta, Operation>(
            Operation.Add,
            Operation.Delete,
            Operation.Modify)
            .WhenUpdated(
                "operation.replace.product",
                "Product replacement requires a Replace operation.",
                update => update.Delta.ProductChange.HasChanged,
                Operation.Replace);

        var result = rules.Plan(structural);

        Assert.AreEqual(Operation.Replace, result.Operations.Updates.Single().Operation);
        Assert.IsTrue(result.Explanation.Entries.Single().Reasons.Any(reason => reason.Code == "operation.replace.product"));
    }

    [TestMethod]
    public void Plan_WhenNoSpecificRuleMatches_ShouldUseFallbackUpdate()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var current = new[] { new OrderItem(id, "router", 1) };
        var desired = new[] { new OrderItem(id, "router", 2) };
        var structural = OrderItemSync.Plan(current, desired);
        var rules = new SyncOperationRules<OrderItem, OrderItemSync.Key, OrderItemDelta, Operation>(
            Operation.Add,
            Operation.Delete,
            Operation.Modify)
            .WhenUpdated(
                "operation.replace.product",
                "Product replacement requires a Replace operation.",
                update => update.Delta.ProductChange.HasChanged,
                Operation.Replace);

        var result = rules.Plan(structural);

        Assert.AreEqual(Operation.Modify, result.Operations.Updates.Single().Operation);
    }
}
