using System.Text.Json;
using Forge.Delta;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class ManifestDigestTests
{
    [TestMethod]
    public void ComputeSha256Hex_ShouldIgnoreManifestOperationAndObjectPropertyOrdering()
    {
        var firstObject = JsonDocument.Parse("{\"b\":2,\"a\":1}").RootElement.Clone();
        var secondObject = JsonDocument.Parse("{\"a\":1,\"b\":2}").RootElement.Clone();
        var nullValue = DeltaManifest.SerializeValue(null, null);
        var first = new SyncManifestDocument(
            1,
            "Item",
            [
                new SyncManifestEntry(SyncManifestOperation.Add, "b", nullValue, firstObject, []),
                new SyncManifestEntry(SyncManifestOperation.Add, "a", nullValue, firstObject, [])
            ]);
        var second = new SyncManifestDocument(
            1,
            "Item",
            [
                new SyncManifestEntry(SyncManifestOperation.Add, "a", nullValue, secondObject, []),
                new SyncManifestEntry(SyncManifestOperation.Add, "b", nullValue, secondObject, [])
            ]);

        var firstDigest = ManifestDigest.ComputeSha256Hex(first);
        var secondDigest = ManifestDigest.ComputeSha256Hex(second);

        Assert.AreEqual(firstDigest, secondDigest);
        Assert.IsTrue(ManifestDigest.VerifySha256Hex(first, firstDigest));
    }

    [TestMethod]
    public void ComputeSha256Hex_WhenPlanChanges_ShouldChangeDigest()
    {
        var nullValue = DeltaManifest.SerializeValue(null, null);
        var one = new SyncManifestDocument(
            1,
            "Item",
            [new SyncManifestEntry(SyncManifestOperation.Add, "a", nullValue, DeltaManifest.SerializeValue(1, null), [])]);
        var two = new SyncManifestDocument(
            1,
            "Item",
            [new SyncManifestEntry(SyncManifestOperation.Add, "a", nullValue, DeltaManifest.SerializeValue(2, null), [])]);

        Assert.AreNotEqual(
            ManifestDigest.ComputeSha256Hex(one),
            ManifestDigest.ComputeSha256Hex(two));
    }
}
