namespace Forge.Dogfood.Tests;

[TestClass]
public sealed class CharacteristicUpsertDeltaDogfoodTests
{
    [TestMethod]
    public void Delta_WhenPersistedCharacteristicIsEquivalent_ShouldAvoidWrite()
    {
        var current = new CharacteristicPersistenceState(
            "materiaaltype",
            "\"glas\"",
            "ExternalSystem",
            null,
            true);
        var desired = new CharacteristicPersistenceState(
            "materiaaltype",
            "\"glas\"",
            "ExternalSystem",
            null,
            true);

        Assert.IsTrue(CharacteristicPersistenceStateDelta.AreEquivalent(current, desired));
    }

    [TestMethod]
    public void Delta_WhenTypedValueChanges_ShouldDescribeExactPersistenceChange()
    {
        var current = new CharacteristicPersistenceState(
            "ipSubnet",
            "json:null",
            "ExternalSystem",
            null,
            true);
        var desired = new CharacteristicPersistenceState(
            "ipSubnet",
            "\"10.20.0.0/24\"",
            "ExternalSystem",
            null,
            true);

        var delta = CharacteristicPersistenceStateDelta.Between(current, desired);

        Assert.IsTrue(delta.HasChanges);
        Assert.IsTrue(delta.ValueChange.HasChanged);
        Assert.AreEqual("json:null", delta.ValueChange.Before);
        Assert.AreEqual("\"10.20.0.0/24\"", delta.ValueChange.After);
        Assert.AreEqual(1, delta.Changes.Count);
        Assert.AreEqual("Value", delta.Changes[0].Path);
    }

    [TestMethod]
    public void Delta_WhenOnlyInventoryFlagChanges_ShouldDetectExistingWriteRequirement()
    {
        var current = new CharacteristicPersistenceState(
            "materiaaltype",
            "\"glas\"",
            "ExternalSystem",
            null,
            false);
        var desired = current with { IncludeInInventory = true };

        var delta = CharacteristicPersistenceStateDelta.Between(current, desired);

        Assert.IsTrue(delta.IncludeInInventoryChange.HasChanged);
        Assert.AreEqual(1, delta.Changes.Count);
    }
}
