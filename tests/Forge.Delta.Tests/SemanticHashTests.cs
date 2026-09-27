namespace Forge.Delta.Tests;

[TestClass]
public sealed class SemanticHashTests
{
    [TestMethod]
    public void GetSemanticHashCode_WhenValuesAreEquivalent_ShouldMatch()
    {
        var id = Guid.NewGuid();
        var first = new Customer(id, "Pjotr", "x@example.com", new Address("Apeldoorn", "1"));
        var second = new Customer(id, "PJOTR", "x@example.com", new Address("Apeldoorn", "1"));

        Assert.IsTrue(CustomerDelta.AreEquivalent(first, second));
        Assert.AreEqual(
            CustomerDelta.GetSemanticHashCode(first),
            CustomerDelta.GetSemanticHashCode(second));
    }
}
