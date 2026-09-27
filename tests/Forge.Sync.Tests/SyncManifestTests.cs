namespace Forge.Sync.Tests;

[TestClass]
public sealed class SyncManifestTests
{
    [TestMethod]
    public void Create_ShouldProducePortableRoundTrippableManifest()
    {
        var existingId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var removedId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var addedId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        OrderItem[] current = [new(existingId, "router", 1), new(removedId, "legacy", 1)];
        OrderItem[] desired = [new(existingId, "router", 2), new(addedId, "firewall", 1)];
        var plan = OrderItemSync.Plan(current, desired);

        var manifest = SyncManifest.Create(plan, static key => key.ToString());
        var json = manifest.ToJson();
        var parsed = SyncManifestDocument.Parse(json);

        Assert.AreEqual(SyncManifestDocument.CurrentSchemaVersion, parsed.SchemaVersion);
        Assert.AreEqual(3, parsed.OperationCount);
        Assert.IsTrue(parsed.Operations.Any(static operation =>
            operation.Operation == SyncManifestOperation.Update && operation.Changes.Count != 0));
    }
}
