namespace Forge.Delta.Tests;

[TestClass]
public sealed class NestedNullableDeltaTests
{
    [TestMethod]
    public void Between_WhenNullableNestedReferenceTransitionsToNull_ReportsContainingProperty()
    {
        var result = CustomerProfileDelta.Between(
            new CustomerProfile(new Address("Apeldoorn", "NL")),
            new CustomerProfile(null));

        Assert.IsTrue(result.HasChanges);
        Assert.IsTrue(result.AddressChange.HasChanged);
        Assert.IsNull(result.AddressDelta);
        Assert.AreEqual("Address", result.Changes.Single().Path);
    }

    [TestMethod]
    public void Between_WhenNullableNestedReferenceValuesAreEquivalent_ReturnsNoChanges()
    {
        var result = CustomerProfileDelta.Between(
            new CustomerProfile(new Address("Apeldoorn", "NL")),
            new CustomerProfile(new Address("Apeldoorn", "NL")));

        Assert.IsFalse(result.HasChanges);
        Assert.IsNotNull(result.AddressDelta);
        Assert.IsFalse(result.AddressDelta!.HasChanges);
    }

    [TestMethod]
    public void Between_WhenNullableNestedStructChanges_ReturnsFlattenedPath()
    {
        var result = LocationDelta.Between(
            new Location(new Coordinates(1, 2)),
            new Location(new Coordinates(3, 2)));

        Assert.IsTrue(result.HasChanges);
        Assert.IsNotNull(result.CoordinatesDelta);
        Assert.IsTrue(result.CoordinatesDelta!.XChange.HasChanged);
        Assert.AreEqual("Coordinates.X", result.Changes.Single().Path);
    }

    [TestMethod]
    public void Between_WhenNullableNestedStructTransitionsToNull_ReportsContainingProperty()
    {
        var result = LocationDelta.Between(
            new Location(new Coordinates(1, 2)),
            new Location(null));

        Assert.IsTrue(result.HasChanges);
        Assert.IsNull(result.CoordinatesDelta);
        Assert.AreEqual("Coordinates", result.Changes.Single().Path);
    }
}
