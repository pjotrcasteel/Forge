namespace Forge.Sync.Tests;

[TestClass]
public sealed class ServiceReferenceSyncTests
{
    [TestMethod]
    public void Plan_WithCustomKeyComparer_TreatsEquivalentKeysAsSameLogicalItem()
    {
        IReadOnlyList<ServiceReference> current =
        [
            new ServiceReference("RFS-001", "pending")
        ];
        IReadOnlyList<ServiceReference> desired =
        [
            new ServiceReference("rfs-001", "active")
        ];

        var result = ServiceReferenceSync.Plan(current, desired);

        Assert.AreEqual(0, result.Added.Count);
        Assert.AreEqual(0, result.Removed.Count);
        Assert.AreEqual(1, result.Updated.Count);
        Assert.AreEqual("RFS-001", result.Updated[0].Current.ExternalId);
        Assert.AreEqual("rfs-001", result.Updated[0].Desired.ExternalId);
        Assert.AreEqual(
            ServiceReferenceSync.GetKey(current[0]),
            ServiceReferenceSync.GetKey(desired[0]));
    }

    [TestMethod]
    public void GetKey_ReturnsStronglyTypedLogicalIdentity()
    {
        var item = new ServiceReference("RFS-001", "active");

        var key = ServiceReferenceSync.GetKey(item);

        Assert.AreEqual("RFS-001", key.ExternalId);
        Assert.AreEqual("ExternalId=RFS-001", key.ToString());
    }
    [TestMethod]
    public void Plan_WithCustomKeyComparer_RejectsEquivalentDuplicateDesiredKeys()
    {
        IReadOnlyList<ServiceReference> desired =
        [
            new ServiceReference("RFS-001", "pending"),
            new ServiceReference("rfs-001", "active")
        ];

        var exception = Assert.ThrowsExactly<DuplicateSyncKeyException>(
            () => ServiceReferenceSync.Plan(Array.Empty<ServiceReference>(), desired));

        Assert.AreEqual("desired", exception.CollectionName);
        StringAssert.Contains(exception.Key, "ExternalId=");
    }

    [TestMethod]
    public void Plan_WhenOnlyEquivalentKeyRepresentationDiffers_IsUnchanged()
    {
        IReadOnlyList<ServiceReference> current =
        [
            new ServiceReference("RFS-001", "active")
        ];
        IReadOnlyList<ServiceReference> desired =
        [
            new ServiceReference("rfs-001", "active")
        ];

        var result = ServiceReferenceSync.Plan(current, desired);

        Assert.AreEqual(0, result.Updated.Count);
        Assert.AreEqual(1, result.Unchanged.Count);
    }

}
