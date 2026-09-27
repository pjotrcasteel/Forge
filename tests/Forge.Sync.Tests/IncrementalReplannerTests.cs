using Forge.Sync;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class IncrementalReplannerTests
{
    [TestMethod]
    public void Replan_WhenOperationIsSemanticallyUnchanged_ShouldRetainOperationIdentity()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var current = new[] { new OrderItem(id, "router", 1) };
        var desired = new[] { new OrderItem(id, "router", 2) };
        var manifest = SyncManifest.Create(OrderItemSync.Plan(current, desired), key => key.ToString());
        var sequence = 0;
        var initial = IncrementalReplanner.Create(manifest, _ => "op-" + ++sequence);

        var result = IncrementalReplanner.Replan(initial, manifest, _ => "op-" + ++sequence);

        Assert.AreEqual(1, result.Retained.Count);
        Assert.AreEqual("op-1", result.Plan.Operations.Single().OperationId);
        Assert.AreEqual(0, result.Replaced.Count);
    }

    [TestMethod]
    public void Replan_WhenDesiredUpdateChanges_ShouldReplaceOperationIdentity()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var current = new[] { new OrderItem(id, "router", 1) };
        var firstDesired = new[] { new OrderItem(id, "router", 2) };
        var secondDesired = new[] { new OrderItem(id, "router", 3) };
        var initialManifest = SyncManifest.Create(OrderItemSync.Plan(current, firstDesired), key => key.ToString());
        var nextManifest = SyncManifest.Create(OrderItemSync.Plan(current, secondDesired), key => key.ToString());
        var sequence = 0;
        var initial = IncrementalReplanner.Create(initialManifest, _ => "op-" + ++sequence);

        var result = IncrementalReplanner.Replan(initial, nextManifest, _ => "op-" + ++sequence);

        Assert.AreEqual(1, result.Replaced.Count);
        Assert.AreEqual("op-1", result.Replaced[0].Previous.OperationId);
        Assert.AreEqual("op-2", result.Replaced[0].Current.OperationId);
    }

    [TestMethod]
    public void Replan_WhenOperationDisappears_ShouldReportNoLongerRequired()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var current = new[] { new OrderItem(id, "router", 1) };
        var desired = new[] { new OrderItem(id, "router", 2) };
        var initialManifest = SyncManifest.Create(OrderItemSync.Plan(current, desired), key => key.ToString());
        var emptyManifest = SyncManifest.Create(OrderItemSync.Plan(desired, desired), key => key.ToString());
        var initial = IncrementalReplanner.Create(initialManifest, _ => "op-1");

        var result = IncrementalReplanner.Replan(initial, emptyManifest, _ => "unused");

        Assert.AreEqual(1, result.NoLongerRequired.Count);
        Assert.AreEqual(0, result.Plan.Operations.Count);
    }
}
