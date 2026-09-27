namespace Forge.Sync.Tests;

[TestClass]
public sealed class ManifestPreconditionTests
{
    [TestMethod]
    public void Validate_WhenCurrentStateStillMatches_ShouldSucceed()
    {
        OrderItem[] current = [new("a", "router", 1)];
        OrderItem[] desired = [new("a", "router", 2), new("b", "firewall", 1)];
        var plan = OrderItemSync.Plan(current, desired);
        var manifest = SyncManifest.Create(plan, static key => key.ToString());
        var snapshot = ManifestStateSnapshot.Create(
            current,
            static item => OrderItemSync.GetKey(item),
            static key => key.ToString());

        var result = ManifestPreconditions.Validate(manifest, snapshot);

        Assert.IsTrue(result.IsSatisfied);
    }

    [TestMethod]
    public void Validate_WhenStateChangedAfterPlanning_ShouldRejectStalePlan()
    {
        OrderItem[] current = [new("a", "router", 1)];
        OrderItem[] desired = [new("a", "router", 2)];
        var manifest = SyncManifest.Create(OrderItemSync.Plan(current, desired), static key => key.ToString());
        OrderItem[] changedMeanwhile = [new("a", "router", 99)];
        var snapshot = ManifestStateSnapshot.Create(
            changedMeanwhile,
            static item => OrderItemSync.GetKey(item),
            static key => key.ToString());

        var result = ManifestPreconditions.Validate(manifest, snapshot);

        Assert.IsFalse(result.IsSatisfied);
        Assert.AreEqual(ManifestPreconditionFailureReason.CurrentStateChanged, result.Failures[0].Reason);
    }
}
