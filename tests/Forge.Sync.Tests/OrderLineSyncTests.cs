namespace Forge.Sync.Tests;

[TestClass]
public sealed class OrderLineSyncTests
{
    [TestMethod]
    public void Plan_WithCompositeKey_MatchesByAllConfiguredProperties()
    {
        var orderId = Guid.NewGuid();
        IReadOnlyList<OrderLine> current =
        [
            new OrderLine(orderId, 1, "Router", 1),
            new OrderLine(orderId, 2, "Cable", 1)
        ];

        IReadOnlyList<OrderLine> desired =
        [
            new OrderLine(orderId, 1, "Router", 2),
            new OrderLine(orderId, 3, "Bracket", 1)
        ];

        var result = OrderLineSync.Plan(current, desired);

        Assert.AreEqual(1, result.Updated.Count);
        Assert.AreEqual(1, result.Updated[0].Current.LineNumber);
        Assert.AreEqual(1, result.Added.Count);
        Assert.AreEqual(3, result.Added[0].Desired.LineNumber);
        Assert.AreEqual(1, result.Removed.Count);
        Assert.AreEqual(2, result.Removed[0].Current.LineNumber);
    }
}