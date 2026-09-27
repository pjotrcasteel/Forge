using Forge.Delta;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class CrossSyncMatchTests
{
    [TestMethod]
    public void Plan_WhenCanonicalKeyMatches_ShouldPreferItOverFallbackKey()
    {
        var primaryId = Guid.NewGuid();
        CurrentItem[] current =
        [
            new(primaryId, "alpha", "old-primary"),
            new(Guid.NewGuid(), "beta", "old-fallback")
        ];
        DesiredItem[] desired = [new(primaryId, "beta", "new")];

        var plan = CrossSync.Plan(current, desired, Definition());

        Assert.AreEqual(1, plan.Updated.Count);
        Assert.AreSame(current[0], plan.Updated[0].Current);
        Assert.AreEqual(IdKey(primaryId), plan.Updated[0].Key);
        Assert.AreEqual(1, plan.Removed.Count);
        Assert.AreSame(current[1], plan.Removed[0].Current);
    }

    [TestMethod]
    public void Plan_WhenCanonicalKeyDoesNotMatch_ShouldUseOrderedFallbackKey()
    {
        var currentId = Guid.NewGuid();
        CurrentItem[] current = [new(currentId, "alpha", "old")];
        DesiredItem[] desired = [new(Guid.Empty, "ALPHA", "new")];

        var plan = CrossSync.Plan(current, desired, Definition());

        Assert.AreEqual(1, plan.Updated.Count);
        Assert.AreSame(current[0], plan.Updated[0].Current);
        Assert.AreEqual(IdKey(currentId), plan.Updated[0].Key);
        Assert.AreEqual(0, plan.Added.Count);
        Assert.AreEqual(0, plan.Removed.Count);
    }

    [TestMethod]
    public void Plan_WhenFallbackIdentityIsAmbiguous_ShouldFail()
    {
        CurrentItem[] current =
        [
            new(Guid.NewGuid(), "alpha", "one"),
            new(Guid.NewGuid(), "ALPHA", "two")
        ];
        DesiredItem[] desired = [new(Guid.Empty, "alpha", "desired")];

        Assert.ThrowsExactly<AmbiguousSyncIdentityException>(() =>
            CrossSync.Plan(current, desired, Definition()));
    }

    [TestMethod]
    public void Plan_WhenTwoDesiredItemsResolveToSameCurrentItem_ShouldFail()
    {
        var currentId = Guid.NewGuid();
        CurrentItem[] current = [new(currentId, "alpha", "old")];
        DesiredItem[] desired =
        [
            new(currentId, "alpha", "first"),
            new(Guid.Empty, "alpha", "second")
        ];

        Assert.ThrowsExactly<DuplicateSyncMatchException>(() =>
            CrossSync.Plan(current, desired, Definition()));
    }

    [TestMethod]
    public void Plan_WhenReplaceMode_ShouldRemoveUnmatchedCurrentItems()
    {
        CurrentItem[] current = [new(Guid.NewGuid(), "remove", "old")];
        DesiredItem[] desired = [];

        var plan = CrossSync.Plan(current, desired, Definition());

        Assert.AreEqual(1, plan.Removed.Count);
        Assert.AreEqual(IdKey(current[0].Id), plan.Removed[0].Key);
    }

    private static CrossSyncMatchDefinition<CurrentItem, DesiredItem, string, TestDelta> Definition()
        => new(
            static item => new SyncIdentity<string>(
                IdKey(item.Id),
                [CharacteristicKey(item.CharacteristicId)]),
            static item => item.Id == Guid.Empty
                ? new SyncIdentity<string>(CharacteristicKey(item.CharacteristicId))
                : new SyncIdentity<string>(
                    IdKey(item.Id),
                    [CharacteristicKey(item.CharacteristicId)]),
            static (current, desired) => current.Value == desired.Value,
            static (current, desired) => new TestDelta(current.Value, desired.Value),
            SyncMode.Replace,
            StringComparer.OrdinalIgnoreCase);

    private static string IdKey(Guid id) => $"id:{id:D}";

    private static string CharacteristicKey(string characteristicId) => $"characteristic:{characteristicId}";

    private sealed record CurrentItem(Guid Id, string CharacteristicId, string Value);
    private sealed record DesiredItem(Guid Id, string CharacteristicId, string Value);

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
