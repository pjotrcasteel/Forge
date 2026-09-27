namespace Forge.Delta.Tests;

[TestClass]
public sealed class DeltaInvertTests
{
    [TestMethod]
    public void Invert_ShouldSwapBeforeAndAfterIncludingNestedDelta()
    {
        var before = new Customer(Guid.NewGuid(), "Pjotr", "old@example.com", new Address("Apeldoorn", "1"));
        var after = before with { Email = "new@example.com", Address = new Address("Amsterdam", "1") };

        var inverted = CustomerDelta.Between(before, after).Invert();

        Assert.AreEqual("new@example.com", inverted.EmailChange.Before);
        Assert.AreEqual("old@example.com", inverted.EmailChange.After);
        Assert.AreEqual("Amsterdam", inverted.AddressDelta!.CityChange.Before);
        Assert.AreEqual("Apeldoorn", inverted.AddressDelta.CityChange.After);
    }
}
