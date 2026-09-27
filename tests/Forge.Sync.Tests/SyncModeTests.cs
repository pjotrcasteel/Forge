using Forge.Sync;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class SyncModeTests
{
    [TestMethod]
    public void Plan_InReplaceMode_ShouldRemoveCurrentItemsMissingFromDesired()
    {
        IReadOnlyList<OrderItem> current =
        [
            new OrderItem(FirstId, "one", 1),
            new OrderItem(SecondId, "two", 2),
        ];
        IReadOnlyList<OrderItem> desired = [new OrderItem(FirstId, "one", 1)];

        var plan = OrderItemSync.Plan(current, desired, SyncMode.Replace);

        Assert.AreEqual(1, plan.Removed.Count);
        Assert.AreEqual(0, plan.Preserved.Count);
        Assert.AreEqual(2, plan.CurrentCount);
        Assert.AreEqual(1, plan.DesiredCount);
    }

    [TestMethod]
    public void Plan_InUpsertMode_ShouldPreserveCurrentItemsMissingFromPayload()
    {
        IReadOnlyList<OrderItem> current =
        [
            new OrderItem(FirstId, "one", 1),
            new OrderItem(SecondId, "two", 2),
        ];
        IReadOnlyList<OrderItem> desired = [new OrderItem(FirstId, "one", 3)];

        var plan = OrderItemSync.Plan(current, desired, SyncMode.Upsert);

        Assert.AreEqual(0, plan.Removed.Count);
        Assert.AreEqual(1, plan.Preserved.Count);
        Assert.AreEqual(SecondId, plan.Preserved[0].Current.Id);
        Assert.AreEqual(1, plan.Updated.Count);
        Assert.IsTrue(plan.HasChanges);
    }

    [TestMethod]
    public void Plan_DefaultOverload_ShouldUseReplaceSemantics()
    {
        IReadOnlyList<OrderItem> current = [new OrderItem(FirstId, "one", 1)];

        var plan = OrderItemSync.Plan(current, []);

        Assert.AreEqual(1, plan.Removed.Count);
        Assert.AreEqual(0, plan.Preserved.Count);
    }

    [TestMethod]
    public void AreEquivalent_InReplaceMode_WhenCurrentContainsExtraItem_ShouldReturnFalse()
    {
        IReadOnlyList<OrderItem> current =
        [
            new OrderItem(FirstId, "one", 1),
            new OrderItem(SecondId, "two", 2),
        ];
        IReadOnlyList<OrderItem> desired = [new OrderItem(FirstId, "one", 1)];

        var equivalent = OrderItemSync.AreEquivalent(current, desired, SyncMode.Replace);

        Assert.IsFalse(equivalent);
    }

    [TestMethod]
    public void AreEquivalent_InUpsertMode_WhenPayloadMatchesSubset_ShouldReturnTrue()
    {
        IReadOnlyList<OrderItem> current =
        [
            new OrderItem(FirstId, "one", 1),
            new OrderItem(SecondId, "two", 2),
        ];
        IReadOnlyList<OrderItem> desired = [new OrderItem(FirstId, "one", 1)];

        var equivalent = OrderItemSync.AreEquivalent(current, desired, SyncMode.Upsert);

        Assert.IsTrue(equivalent);
    }

    [TestMethod]
    public void AreEquivalent_InUpsertMode_WhenPayloadChangesExistingItem_ShouldReturnFalse()
    {
        IReadOnlyList<OrderItem> current = [new OrderItem(FirstId, "one", 1)];
        IReadOnlyList<OrderItem> desired = [new OrderItem(FirstId, "one", 2)];

        var equivalent = OrderItemSync.AreEquivalent(current, desired, SyncMode.Upsert);

        Assert.IsFalse(equivalent);
    }

    private static readonly Guid FirstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SecondId = Guid.Parse("22222222-2222-2222-2222-222222222222");
}
