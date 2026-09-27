namespace Forge.Delta.Tests;

[TestClass]
public sealed class CustomerDeltaTests
{
    [TestMethod]
    public void Between_WhenValuesAreEqual_ReturnsNoChanges()
    {
        var id = Guid.NewGuid();
        var before = new Customer(id, "Ada", "ada@example.test", new Address("Apeldoorn", "NL"));
        var after = before with { Address = new Address("Apeldoorn", "NL") };

        var result = CustomerDelta.Between(before, after);

        Assert.IsFalse(result.HasChanges);
        Assert.AreEqual(0, result.Changes.Count);
        Assert.IsFalse(result.NameChange.HasChanged);
        Assert.IsFalse(result.EmailChange.HasChanged);
        Assert.IsNotNull(result.AddressDelta);
        Assert.IsFalse(result.AddressDelta!.HasChanges);
    }

    [TestMethod]
    public void Between_WhenPropertiesChange_ReturnsTypedAndUntypedChanges()
    {
        var id = Guid.NewGuid();
        var before = new Customer(
            id,
            "Ada",
            "ada@old.test",
            new Address("Apeldoorn", "NL"));

        var after = before with
        {
            Name = "Ada Lovelace",
            Email = null
        };

        var result = CustomerDelta.Between(before, after);

        Assert.IsTrue(result.HasChanges);
        Assert.AreEqual(2, result.Changes.Count);
        Assert.AreEqual("Ada", result.NameChange.Before);
        Assert.AreEqual("Ada Lovelace", result.NameChange.After);
        Assert.AreEqual("ada@old.test", result.EmailChange.Before);
        Assert.IsNull(result.EmailChange.After);
        CollectionAssert.AreEqual(
            new[] { "Name", "Email" },
            result.Changes.Select(change => change.Path).ToArray());
    }

    [TestMethod]
    public void Between_WhenCustomComparerConsidersValuesEqual_DoesNotReportChange()
    {
        var customer = new Customer(
            Guid.NewGuid(),
            "Ada",
            "ada@example.test",
            new Address("Apeldoorn", "NL"));

        var result = CustomerDelta.Between(customer, customer with { Name = "ADA" });

        Assert.IsFalse(result.HasChanges);
        Assert.IsFalse(result.NameChange.HasChanged);
    }

    [TestMethod]
    public void Between_WhenIgnoredPropertyChanges_DoesNotReportChange()
    {
        var customer = new Customer(
            Guid.NewGuid(),
            "Ada",
            "ada@example.test",
            new Address("Apeldoorn", "NL"));

        var result = CustomerDelta.Between(
            customer,
            customer with { LastModified = DateTimeOffset.UtcNow.AddDays(1) });

        Assert.IsFalse(result.HasChanges);
    }

    [TestMethod]
    public void Between_WhenNestedObjectChanges_ReturnsNestedDeltaAndFlattenedPath()
    {
        var customer = new Customer(
            Guid.NewGuid(),
            "Ada",
            "ada@example.test",
            new Address("Apeldoorn", "NL"));

        var result = CustomerDelta.Between(
            customer,
            customer with { Address = new Address("Amsterdam", "NL") });

        Assert.IsTrue(result.AddressChange.HasChanged);
        Assert.IsNotNull(result.AddressDelta);
        Assert.IsTrue(result.AddressDelta!.CityChange.HasChanged);
        Assert.AreEqual(1, result.Changes.Count);
        Assert.AreEqual("Address.City", result.Changes[0].Path);
        Assert.AreEqual("Apeldoorn", result.Changes[0].Before);
        Assert.AreEqual("Amsterdam", result.Changes[0].After);
    }

    [TestMethod]
    public void Between_WhenBeforeIsNull_ThrowsExactly()
    {
        var after = new Customer(
            Guid.NewGuid(),
            "Ada",
            null,
            new Address("Apeldoorn", "NL"));

        Assert.ThrowsExactly<ArgumentNullException>(() => CustomerDelta.Between(null!, after));
    }
    [TestMethod]
    public void AreEquivalent_WhenNestedStateMatches_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var before = new Customer(id, "Ada", "ada@example.test", new Address("Apeldoorn", "NL"));
        var after = before with
        {
            Name = "ADA",
            Address = new Address("Apeldoorn", "NL")
        };

        var result = CustomerDelta.AreEquivalent(before, after);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void AreEquivalent_WhenNestedStateChanges_ReturnsFalse()
    {
        var customer = new Customer(
            Guid.NewGuid(),
            "Ada",
            "ada@example.test",
            new Address("Apeldoorn", "NL"));

        var result = CustomerDelta.AreEquivalent(
            customer,
            customer with { Address = new Address("Amsterdam", "NL") });

        Assert.IsFalse(result);
    }

}
