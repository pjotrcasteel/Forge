using Forge.Sync;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class ManifestPlanSimulatorTests
{
    [TestMethod]
    public void Simulate_WhenPlanIsApplicable_ShouldProjectExactDesiredReplaceState()
    {
        var firstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var removedId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var addedId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var current = new[]
        {
            new OrderItem(firstId, "router", 1),
            new OrderItem(removedId, "switch", 1)
        };
        var desired = new[]
        {
            new OrderItem(firstId, "router", 2),
            new OrderItem(addedId, "firewall", 1)
        };
        var plan = OrderItemSync.Plan(current, desired);
        var manifest = SyncManifest.Create(plan, key => key.ToString());
        var currentSnapshot = ManifestStateSnapshot.Create(current, OrderItemSync.GetKey, key => key.ToString());
        var desiredSnapshot = ManifestStateSnapshot.Create(desired, OrderItemSync.GetKey, key => key.ToString());

        var simulation = ManifestPlanSimulator.Simulate(manifest, currentSnapshot);

        Assert.IsTrue(simulation.Applied);
        Assert.IsTrue(simulation.Matches(desiredSnapshot));
    }

    [TestMethod]
    public void Simulate_WhenUpsertPreservesCurrentOnlyState_ShouldKeepItInProjection()
    {
        var firstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var preservedId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var current = new[]
        {
            new OrderItem(firstId, "router", 1),
            new OrderItem(preservedId, "switch", 1)
        };
        var patch = new[] { new OrderItem(firstId, "router", 2) };
        var plan = OrderItemSync.Plan(current, patch, SyncMode.Upsert);
        var manifest = SyncManifest.Create(plan, key => key.ToString());
        var snapshot = ManifestStateSnapshot.Create(current, OrderItemSync.GetKey, key => key.ToString());

        var simulation = ManifestPlanSimulator.Simulate(manifest, snapshot);

        Assert.IsTrue(simulation.Applied);
        Assert.AreEqual(2, simulation.ProjectedState.Count);
        Assert.IsTrue(simulation.ProjectedState.ContainsKey(OrderItemSync.GetKey(current[1]).ToString()));
    }

    [TestMethod]
    public void Simulate_WhenCurrentStateChangedAfterPlanning_ShouldRefuseToApply()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var current = new[] { new OrderItem(id, "router", 1) };
        var desired = new[] { new OrderItem(id, "router", 2) };
        var changedAfterPlanning = new[] { new OrderItem(id, "router", 99) };
        var manifest = SyncManifest.Create(OrderItemSync.Plan(current, desired), key => key.ToString());
        var staleSnapshot = ManifestStateSnapshot.Create(
            changedAfterPlanning,
            OrderItemSync.GetKey,
            key => key.ToString());

        var simulation = ManifestPlanSimulator.Simulate(manifest, staleSnapshot);

        Assert.IsFalse(simulation.Applied);
        Assert.IsFalse(simulation.Preconditions.IsSatisfied);
        Assert.AreEqual(99, simulation.ProjectedState.Values.Single().GetProperty("Quantity").GetInt32());
    }
}
