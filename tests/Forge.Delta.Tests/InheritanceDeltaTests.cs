namespace Forge.Delta.Tests;

[TestClass]
public sealed class InheritanceDeltaTests
{
    [TestMethod]
    public void Between_WhenBasePropertyChanges_ReportsInheritedProperty()
    {
        var before = new ProvisionedService(Guid.NewGuid(), "Pending");
        var after = new ProvisionedService(Guid.NewGuid(), "Pending");

        var result = ProvisionedServiceDelta.Between(before, after);

        Assert.IsTrue(result.IdChange.HasChanged);
        Assert.IsFalse(result.StateChange.HasChanged);
        Assert.AreEqual("Id", result.Changes[0].Path);
    }
    [TestMethod]
    public void Between_WhenDerivedOverrideIsIgnored_DoesNotRetainBaseProperty()
    {
        var result = BusinessEntityDelta.Between(
            new BusinessEntity("old", "Internet"),
            new BusinessEntity("new", "Internet"));

        Assert.IsFalse(result.HasChanges);
    }

}
