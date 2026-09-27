using Forge.Sync;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class GeneratedCrossSyncProfileTests
{
    [TestMethod]
    public void Plan_ShouldReconcileDifferentClrTypesWithoutManualDefinition()
    {
        var current = new[] { new StoredLine("A", "router", 1) };
        var desired = new[] { new RequestedLine("A", "router", 2), new RequestedLine("B", "switch", 1) };

        var plan = StoredLineProfile.Plan(current, desired);

        Assert.AreEqual(1, plan.Added.Count);
        Assert.AreEqual(1, plan.Updated.Count);
        Assert.AreEqual("Quantity", plan.Updated[0].Delta.Changes.Single().Path);
    }
}

public sealed record StoredLine(string Id, string Name, int Quantity);
public sealed record RequestedLine(string Id, string Name, int Quantity);

[GenerateCrossSyncProfile(typeof(StoredLine), typeof(RequestedLine))]
[CrossSyncIdentity(nameof(StoredLine.Id), nameof(RequestedLine.Id))]
[CrossSyncMap(nameof(StoredLine.Name), nameof(RequestedLine.Name))]
[CrossSyncMap(nameof(StoredLine.Quantity), nameof(RequestedLine.Quantity))]
public partial class StoredLineProfile
{
}
