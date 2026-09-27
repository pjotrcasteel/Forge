using Forge.Sync;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class PlanExplanationTests
{
    [TestMethod]
    public void Explain_WhenItemUpdated_ShouldIncludePropertyReason()
    {
        var current = new[] { new OrderItem(Guid.Parse("11111111-1111-1111-1111-111111111111"), "router", 1) };
        var desired = new[] { new OrderItem(current[0].Id, "router", 2) };

        var plan = OrderItemSync.Plan(current, desired);
        var explanation = SyncPlanExplainer.Explain(plan);

        var entry = explanation.Entries.Single();
        Assert.AreEqual(PlanExplanationAction.Updated, entry.Action);
        var changed = entry.Reasons.Single().Children.Single();
        Assert.AreEqual("Quantity", changed.Path);
        Assert.AreEqual(1, changed.Before);
        Assert.AreEqual(2, changed.After);
    }

    [TestMethod]
    public void Builder_ShouldAddApplicationOwnedDependencyReason()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var plan = OrderItemSync.Plan(Array.Empty<OrderItem>(), [new OrderItem(id, "router", 1)]);
        var explanation = SyncPlanExplainer.Explain(plan);

        var enriched = new PlanExplanationBuilder<OrderItemSync.Key>(explanation)
            .AddReason(
                OrderItemSync.GetKey(new OrderItem(id, "router", 1)),
                new PlanExplanationNode("dependency.wait", "Broadband must be Completed first."))
            .Build();

        Assert.AreEqual(2, enriched.Entries.Single().Reasons.Count);
        Assert.AreEqual("dependency.wait", enriched.Entries.Single().Reasons[1].Code);
    }
}
