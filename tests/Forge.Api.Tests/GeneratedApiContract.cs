using Forge.Delta;
using Forge.Sync;

namespace Forge.Api.Tests;

internal static class GeneratedApiContract
{
    public static void CompileDeltaContract(ApiCustomer baseline, ApiCustomer current, ApiCustomer desired)
    {
        var equivalent = ApiCustomerDelta.AreEquivalent(baseline, current);
        var delta = ApiCustomerDelta.Between(baseline, current);
        var merge = ApiCustomerDelta.AnalyzeMerge(baseline, current, desired);
        var inverted = delta.Invert();
        var semanticHash = ApiCustomerDelta.GetSemanticHashCode(current);
        ValueChange<string> nameChange = delta.NameChange;
        IReadOnlyList<PropertyChange> changes = delta.Changes;

        _ = equivalent;
        _ = nameChange;
        _ = changes;
        _ = delta.AddressDelta;
        _ = merge.Conflicts;
        _ = inverted;
        _ = semanticHash;
    }

    public static void CompileSyncContract(
        IReadOnlyList<ApiItem> current,
        IReadOnlyList<ApiItem> desired)
    {
        ApiItemSync.Key key = ApiItemSync.GetKey(current[0]);
        var equivalent = ApiItemSync.AreEquivalent(current, desired);
        var equivalentUpsert = ApiItemSync.AreEquivalent(current, desired, SyncMode.Upsert);
        SyncPlan<ApiItem, ApiItemSync.Key, ApiItemDelta> plan = ApiItemSync.Plan(current, desired);
        SyncPlan<ApiItem, ApiItemSync.Key, ApiItemDelta> upsertPlan = ApiItemSync.Plan(
            current,
            desired,
            SyncMode.Upsert);
        SyncAddition<ApiItem, ApiItemSync.Key>? addition = plan.Added.FirstOrDefault();
        SyncRemoval<ApiItem, ApiItemSync.Key>? removal = plan.Removed.FirstOrDefault();
        SyncUpdate<ApiItem, ApiItemSync.Key, ApiItemDelta>? update = plan.Updated.FirstOrDefault();
        SyncUnchanged<ApiItem, ApiItemSync.Key>? unchanged = plan.Unchanged.FirstOrDefault();
        SyncPreserved<ApiItem, ApiItemSync.Key>? preserved = upsertPlan.Preserved.FirstOrDefault();
        var reverse = SyncPlanInverter.Invert(plan, static delta => delta.Invert());
        var manifest = SyncManifest.Create(plan, static generatedKey => generatedKey.ToString());

        _ = key;
        _ = equivalent;
        _ = equivalentUpsert;
        _ = addition;
        _ = removal;
        _ = update;
        _ = unchanged;
        _ = preserved;
        _ = reverse;
        _ = manifest;
    }
}

[GenerateDelta]
internal sealed record ApiAddress(string City);

[GenerateDelta]
internal sealed record ApiCustomer(
    string Name,
    ApiAddress Address);

[GenerateSync(nameof(ApiItem.Id))]
internal sealed record ApiItem(
    int Id,
    string Name);
