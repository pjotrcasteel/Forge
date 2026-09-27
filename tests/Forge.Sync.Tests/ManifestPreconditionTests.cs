namespace Forge.Sync.Tests;

[TestClass]
public sealed class ManifestPreconditionTests
{
    [TestMethod]
    public void Validate_WhenCurrentStateStillMatches_ShouldSucceed()
    {
        var existingId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var addedId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        OrderItem[] current = [new(existingId, "router", 1)];
        OrderItem[] desired = [new(existingId, "router", 2), new(addedId, "firewall", 1)];
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
        var existingId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        OrderItem[] current = [new(existingId, "router", 1)];
        OrderItem[] desired = [new(existingId, "router", 2)];
        var manifest = SyncManifest.Create(OrderItemSync.Plan(current, desired), static key => key.ToString());
        OrderItem[] changedMeanwhile = [new(existingId, "router", 99)];
        var snapshot = ManifestStateSnapshot.Create(
            changedMeanwhile,
            static item => OrderItemSync.GetKey(item),
            static key => key.ToString());

        var result = ManifestPreconditions.Validate(manifest, snapshot);

        Assert.IsFalse(result.IsSatisfied);
        Assert.AreEqual(ManifestPreconditionFailureReason.CurrentStateChanged, result.Failures[0].Reason);
    }
}
