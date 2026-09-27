using Forge.Sync;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class PlanConstraintTests
{
    [TestMethod]
    public void Validate_ShouldReturnAllViolationsWithoutThrowing()
    {
        var protectedId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var otherId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var current = new[]
        {
            new OrderItem(protectedId, "router", 1),
            new OrderItem(otherId, "switch", 1)
        };
        var plan = OrderItemSync.Plan(current, Array.Empty<OrderItem>());
        var constraints = SyncPlanConstraints.For<OrderItem, OrderItemSync.Key, OrderItemDelta>()
            .MaximumChanges(1)
            .RequireNoRemovals()
            .ProtectKey(OrderItemSync.GetKey(current[0]));

        var validation = constraints.Validate(plan);

        Assert.IsFalse(validation.IsValid);
        Assert.AreEqual(3, validation.Violations.Count);
        Assert.IsTrue(validation.Violations.Any(item => item.Code == "sync.protected-key"));
    }

    [TestMethod]
    public void RequireAcyclic_WhenDependencyPlanHasCycle_ShouldReturnViolation()
    {
        var items = new[] { "A", "B" };
        var dependencyPlan = DependencyPlanner.Plan(
            items,
            item => item,
            item => item == "A" ? new[] { "B" } : new[] { "A" });

        var validation = SyncPlanConstraints.RequireAcyclic<string, string>().Validate(dependencyPlan);

        Assert.IsFalse(validation.IsValid);
        Assert.AreEqual("dependency.cycle", validation.Violations.Single().Code);
    }
}
