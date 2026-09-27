namespace Forge.Sync.Tests;

[TestClass]
public sealed class ReconciliationBatchTests
{
    [TestMethod]
    public void Create_ShouldOrderHeterogeneousPlansByDependencies()
    {
        var rfs = Manifest("rfs", 1);
        var resources = Manifest("resources", 2);
        var relationships = Manifest("relationships", 1);

        var batch = ReconciliationBatch.Create(
            [rfs, resources, relationships],
            [
                new PlanDependency("resources", "rfs"),
                new PlanDependency("relationships", "resources")
            ]);

        Assert.IsFalse(batch.HasDependencyCycles);
        Assert.AreEqual("rfs", batch.ExecutionWaves[0].Plans[0].Name);
        Assert.AreEqual("relationships", batch.ExecutionWaves[2].Plans[0].Name);
        Assert.AreEqual(4, batch.TotalOperationCount);
    }

    [TestMethod]
    public void Validate_ShouldCombineStructuralAndApplicationValidation()
    {
        var batch = ReconciliationBatch.Create([Manifest("rfs", 3)], []);

        var result = batch.Validate(static value => value.TotalOperationCount > 2
            ? new BatchValidationIssue("LIMIT", "Too many operations for one approval.")
            : null);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("LIMIT", result.Issues[0].Code);
    }

    private static NamedSyncManifest Manifest(string name, int operations)
    {
        var entries = Enumerable.Range(0, operations)
            .Select(index => new SyncManifestEntry(
                SyncManifestOperation.Add,
                index.ToString(),
                Forge.Delta.DeltaManifest.SerializeValue(null, null),
                Forge.Delta.DeltaManifest.SerializeValue(new { Id = index }, null),
                Array.Empty<Forge.Delta.ChangeManifestEntry>()))
            .ToArray();
        return new NamedSyncManifest(
            name,
            new SyncManifestDocument(SyncManifestDocument.CurrentSchemaVersion, name, entries));
    }
}
