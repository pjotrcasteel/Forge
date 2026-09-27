namespace Forge.Delta.Tests;

[TestClass]
public sealed class ManagedOperationDeltaTests
{
    [TestMethod]
    public void Between_AfterCompensatingOperation_ReportsOnlyDomainStateChanges()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var before = new ManagedOperation("active", "resource-42", timestamp)
        {
            DiagnosticNote = "before"
        };
        var after = new ManagedOperation("removed", null, timestamp)
        {
            DiagnosticNote = "after"
        };

        var delta = ManagedOperationDelta.Between(before, after);

        CollectionAssert.AreEqual(
            new[] { "State", "AssignedResource" },
            delta.Changes.Select(change => change.Path).ToArray());
    }
}
