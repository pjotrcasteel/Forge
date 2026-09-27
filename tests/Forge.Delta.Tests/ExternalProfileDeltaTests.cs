namespace Forge.Delta.Tests;

[TestClass]
public sealed class ExternalProfileDeltaTests
{
    [TestMethod]
    public void ProfileDelta_ShouldApplyIgnoreAndComparerConfigurationWithoutAnnotatingTarget()
    {
        var before = new ExternalProfileCustomer(41, "PJOTR", "old@example.test");
        var after = new ExternalProfileCustomer(42, "pjotr", "new@example.test");

        var delta = ExternalProfileCustomerProfileDelta.Between(before, after);

        Assert.IsTrue(delta.HasChanges);
        Assert.IsFalse(delta.NameChange.HasChanged);
        Assert.IsTrue(delta.EmailChange.HasChanged);
        Assert.AreEqual(1, delta.Changes.Count);
        Assert.AreEqual("Email", delta.Changes[0].Path);
    }
}
