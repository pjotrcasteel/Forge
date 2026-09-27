using Forge.Delta;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class CrossSyncTests
{
    [TestMethod]
    public void Plan_WhenTypesDiffer_ShouldReconcileByLogicalKey()
    {
        CurrentItem[] current = [new("a", "old"), new("remove", "x")];
        DesiredItem[] desired = [new("a", "new"), new("add", "y")];
        var definition = Definition();

        var plan = CrossSync.Plan(current, desired, definition);

        Assert.AreEqual(1, plan.Added.Count);
        Assert.AreEqual("add", plan.Added[0].Key);
        Assert.AreEqual(1, plan.Removed.Count);
        Assert.AreEqual("remove", plan.Removed[0].Key);
        Assert.AreEqual(1, plan.Updated.Count);
        Assert.AreEqual("a", plan.Updated[0].Key);
    }

    [TestMethod]
    public void Plan_WhenUpsert_ShouldPreserveCurrentOnlyItems()
    {
        CurrentItem[] current = [new("keep", "x")];
        DesiredItem[] desired = [];
        var definition = new CrossSyncDefinition<CurrentItem, DesiredItem, string, TestDelta>(
            static item => item.Id,
            static item => item.ExternalId,
            static (left, right) => left.Value == right.Value,
            static (left, right) => new TestDelta(left.Value, right.Value),
            SyncMode.Upsert);

        var plan = CrossSync.Plan(current, desired, definition);

        Assert.AreEqual(0, plan.Removed.Count);
        Assert.AreEqual(1, plan.Preserved.Count);
    }

    private static CrossSyncDefinition<CurrentItem, DesiredItem, string, TestDelta> Definition()
        => new(
            static item => item.Id,
            static item => item.ExternalId,
            static (left, right) => left.Value == right.Value,
            static (left, right) => new TestDelta(left.Value, right.Value));

    private sealed record CurrentItem(string Id, string Value);
    private sealed record DesiredItem(string ExternalId, string Value);

    private sealed class TestDelta : IDelta
    {
        public TestDelta(string before, string after)
        {
            Changes = before == after
                ? []
                : [new PropertyChange("Value", before, after)];
        }

        public bool HasChanges => Changes.Count != 0;
        public IReadOnlyList<PropertyChange> Changes { get; }
    }
}
