using Forge.Delta;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class CrossSyncParityTests
{
    [TestMethod]
    public void CrossTypePlan_ShouldSupportManifestOperationsAndCompensation()
    {
        Current[] current = [new("a", "old")];
        Desired[] desired = [new("a", "new"), new("b", "add")];
        var definition = new CrossSyncDefinition<Current, Desired, string, TestDelta>(
            static item => item.Id,
            static item => item.Id,
            static (left, right) => left.Value == right.Value,
            static (left, right) => new TestDelta(left.Value, right.Value));
        var plan = CrossSync.Plan(current, desired, definition);

        var manifest = CrossSyncManifest.Create(plan, static key => key);
        var operations = CrossSyncOperationPlanner.Classify(
            plan,
            static _ => Operation.Create,
            static _ => Operation.Update,
            static _ => Operation.Delete);
        var reverse = CrossSyncPlanInverter.Invert(plan, static delta => delta.Invert());

        Assert.AreEqual(2, manifest.OperationCount);
        Assert.AreEqual(2, operations.Count);
        Assert.AreEqual(1, reverse.Removed.Count);
    }

    private enum Operation { Create, Update, Delete }
    private sealed record Current(string Id, string Value);
    private sealed record Desired(string Id, string Value);

    private sealed class TestDelta : IDelta
    {
        public TestDelta(string before, string after)
        {
            Before = before;
            After = after;
            Changes = before == after ? [] : [new PropertyChange("Value", before, after)];
        }

        public string Before { get; }
        public string After { get; }
        public bool HasChanges => Changes.Count != 0;
        public IReadOnlyList<PropertyChange> Changes { get; }
        public TestDelta Invert() => new(After, Before);
    }
}
