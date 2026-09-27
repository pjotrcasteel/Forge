using Forge.Delta;
using Forge.Sync;

namespace Forge.Api.Tests;

[TestClass]
public sealed class NestedSyncIdentityTests
{
    [TestMethod]
    public void Between_WhenNestedSyncIdentityChanges_ReportsContainingPropertyChange()
    {
        var before = new ApiContainer(new ApiNestedEntity("entity-1", "same-state"));
        var after = new ApiContainer(new ApiNestedEntity("entity-2", "same-state"));

        var delta = ApiContainerDelta.Between(before, after);

        Assert.IsTrue(delta.HasChanges);
        Assert.IsTrue(delta.EntityChange.HasChanged);
        Assert.IsNotNull(delta.EntityDelta);
        Assert.IsFalse(delta.EntityDelta!.HasChanges);
        Assert.AreEqual("Entity", delta.Changes.Single().Path);
        Assert.IsFalse(ApiContainerDelta.AreEquivalent(before, after));
    }
}

[GenerateSync(nameof(ApiNestedEntity.Id))]
internal sealed record ApiNestedEntity(
    string Id,
    string State);

[GenerateDelta]
internal sealed record ApiContainer(ApiNestedEntity Entity);
