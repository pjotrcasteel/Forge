namespace Forge.Sync.Tests;

[TestClass]
public sealed class SyncManifestTests
{
    [TestMethod]
    public void Create_ShouldProducePortableRoundTrippableManifest()
    {
        OrderItem[] current = [new("a", "router", 1), new("remove", "legacy", 1)];
        OrderItem[] desired = [new("a", "router", 2), new("add", "firewall", 1)];
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
