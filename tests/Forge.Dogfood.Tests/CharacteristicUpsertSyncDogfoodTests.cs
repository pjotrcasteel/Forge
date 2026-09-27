using Forge.Sync;

namespace Forge.Dogfood.Tests;

[TestClass]
public sealed class CharacteristicUpsertSyncDogfoodTests
{
    [TestMethod]
    public void Upsert_WhenPayloadContainsSubset_ShouldNeverTreatAbsentCharacteristicsAsDeletes()
    {
        IReadOnlyList<CharacteristicSyncState> current =
        [
            Characteristic("rfs.materiaaltype", "porselein"),
            Characteristic("rfs.bandbreedte", "100"),
        ];
        IReadOnlyList<CharacteristicSyncState> incoming =
        [
            Characteristic("RFS.MATERIAALTYPE", "glas"),
        ];

        var plan = CharacteristicSyncStateSync.Plan(current, incoming, SyncMode.Upsert);

        Assert.AreEqual(0, plan.Removed.Count);
        Assert.AreEqual(1, plan.Updated.Count);
        Assert.AreEqual(1, plan.Preserved.Count);
        Assert.AreEqual("rfs.bandbreedte", plan.Preserved[0].Current.CharacteristicId);
    }

    private static CharacteristicSyncState Characteristic(string id, string value)
    {
        return new CharacteristicSyncState(
            id,
            id.Split('.').Last(),
            value,
            "ExternalSystem",
            null,
            true);
    }
}
