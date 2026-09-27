namespace Forge.Sync.Tests;

[TestClass]
public sealed class OrderItemSyncTests
{
    [TestMethod]
    public void Plan_CategorizesAddedRemovedUpdatedAndUnchangedItems()
    {
        var removedId = Guid.NewGuid();
        var updatedId = Guid.NewGuid();
        var unchangedId = Guid.NewGuid();
        var addedId = Guid.NewGuid();

        IReadOnlyList<OrderItem> current =
        [
            new OrderItem(removedId, "Removed", 1),
            new OrderItem(updatedId, "Updated", 1),
            new OrderItem(unchangedId, "Unchanged", 3)
        ];

        IReadOnlyList<OrderItem> desired =
        [
            new OrderItem(unchangedId, "Unchanged", 3),
            new OrderItem(updatedId, "Updated", 2),
            new OrderItem(addedId, "Added", 1)
        ];

        var result = OrderItemSync.Plan(current, desired);

        Assert.IsTrue(result.HasChanges);
        Assert.AreEqual(1, result.Added.Count);
        Assert.AreEqual(addedId, result.Added[0].Desired.Id);
        Assert.AreEqual(1, result.Removed.Count);
        Assert.AreEqual(removedId, result.Removed[0].Current.Id);
        Assert.AreEqual(1, result.Updated.Count);
        Assert.AreEqual(updatedId, result.Updated[0].Current.Id);
        Assert.IsTrue(result.Updated[0].Delta.QuantityChange.HasChanged);
        Assert.AreEqual(1, result.Updated[0].Delta.QuantityChange.Before);
        Assert.AreEqual(2, result.Updated[0].Delta.QuantityChange.After);
        Assert.AreEqual(1, result.Unchanged.Count);
        Assert.AreEqual(unchangedId, result.Unchanged[0].Desired.Id);
    }

    [TestMethod]
    public void Plan_PreservesDesiredOrderForDesiredSideCategories()
    {
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();

        IReadOnlyList<OrderItem> current = [];
        IReadOnlyList<OrderItem> desired =
        [
            new OrderItem(firstId, "First", 1),
            new OrderItem(secondId, "Second", 1)
        ];

        var result = OrderItemSync.Plan(current, desired);

        CollectionAssert.AreEqual(
            new[] { firstId, secondId },
            result.Added.Select(item => item.Desired.Id).ToArray());
    }

    [TestMethod]
    public void Plan_WhenCurrentContainsDuplicateKey_ThrowsExactly()
    {
        var id = Guid.NewGuid();
        IReadOnlyList<OrderItem> current =
        [
            new OrderItem(id, "First", 1),
            new OrderItem(id, "Duplicate", 2)
        ];

        var exception = Assert.ThrowsExactly<DuplicateSyncKeyException>(
            () => OrderItemSync.Plan(current, Array.Empty<OrderItem>()));

        Assert.AreEqual("current", exception.CollectionName);
        StringAssert.Contains(exception.Key, "Id=");
    }

    [TestMethod]
    public void Plan_WhenDesiredContainsDuplicateKey_ThrowsExactly()
    {
        var id = Guid.NewGuid();
        IReadOnlyList<OrderItem> desired =
        [
            new OrderItem(id, "First", 1),
            new OrderItem(id, "Duplicate", 2)
        ];

        var exception = Assert.ThrowsExactly<DuplicateSyncKeyException>(
            () => OrderItemSync.Plan(Array.Empty<OrderItem>(), desired));

        Assert.AreEqual("desired", exception.CollectionName);
    }

    [TestMethod]
    public void Plan_WhenNestedValueChanges_ExposesFlattenedDeltaPath()
    {
        var id = Guid.NewGuid();
        IReadOnlyList<OrderItem> current =
        [
            new OrderItem(id, "Internet", 1)
            {
                Configuration = new ItemConfiguration("Standard", true)
            }
        ];
        IReadOnlyList<OrderItem> desired =
        [
            new OrderItem(id, "Internet", 1)
            {
                Configuration = new ItemConfiguration("Premium", true)
            }
        ];

        var result = OrderItemSync.Plan(current, desired);

        Assert.AreEqual(1, result.Updated.Count);
        Assert.IsNotNull(result.Updated[0].Delta.ConfigurationDelta);
        Assert.IsTrue(result.Updated[0].Delta.ConfigurationDelta!.ModeChange.HasChanged);
        Assert.AreEqual("Configuration.Mode", result.Updated[0].Delta.Changes.Single().Path);
    }

    [TestMethod]
    public void Plan_SnapshotsResultCollections()
    {
        var current = new List<OrderItem>();
        var desired = new List<OrderItem>
        {
            new(Guid.NewGuid(), "Internet", 1)
        };

        var result = OrderItemSync.Plan(current, desired);

        current.Clear();
        desired.Clear();

        Assert.AreEqual(1, result.Added.Count);
        Assert.AreEqual(0, result.CurrentCount);
        Assert.AreEqual(1, result.DesiredCount);
    }

    [TestMethod]
    public void Plan_WhenDesiredIsEmpty_ReturnsAllCurrentItemsAsRemovalsInCurrentOrder()
    {
        var first = new OrderItem(Guid.NewGuid(), "First", 1);
        var second = new OrderItem(Guid.NewGuid(), "Second", 1);
        IReadOnlyList<OrderItem> current = [first, second];

        var result = OrderItemSync.Plan(current, Array.Empty<OrderItem>());

        Assert.AreEqual(2, result.Removed.Count);
        Assert.AreEqual(first.Id, result.Removed[0].Current.Id);
        Assert.AreEqual(second.Id, result.Removed[1].Current.Id);
        Assert.AreEqual(2, result.ChangeCount);
    }

    [TestMethod]
    public void AreEquivalent_WhenCurrentContainsDuplicateKey_ThrowsExactly()
    {
        var id = Guid.NewGuid();
        IReadOnlyList<OrderItem> current =
        [
            new OrderItem(id, "First", 1),
            new OrderItem(id, "Duplicate", 2)
        ];

        var exception = Assert.ThrowsExactly<DuplicateSyncKeyException>(
            () => OrderItemSync.AreEquivalent(current, Array.Empty<OrderItem>()));

        Assert.AreEqual("current", exception.CollectionName);
    }

    [TestMethod]
    public void AreEquivalent_WhenDesiredContainsDuplicateKey_ThrowsExactly()
    {
        var id = Guid.NewGuid();
        IReadOnlyList<OrderItem> desired =
        [
            new OrderItem(id, "First", 1),
            new OrderItem(id, "Duplicate", 2)
        ];

        var exception = Assert.ThrowsExactly<DuplicateSyncKeyException>(
            () => OrderItemSync.AreEquivalent(Array.Empty<OrderItem>(), desired));

        Assert.AreEqual("desired", exception.CollectionName);
    }

}