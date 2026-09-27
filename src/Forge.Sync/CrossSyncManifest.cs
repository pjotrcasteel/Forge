using System.Text.Json;
using Forge.Delta;

namespace Forge.Sync;

/// <summary>Creates portable manifests from cross-type reconciliation plans.</summary>
public static class CrossSyncManifest
{
    public static SyncManifestDocument Create<TCurrent, TDesired, TKey, TDelta>(
        CrossSyncPlan<TCurrent, TDesired, TKey, TDelta> plan,
        Func<TKey, string> keyFormatter,
        JsonSerializerOptions? options = null)
        where TDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(keyFormatter);
        var operations = new List<SyncManifestEntry>(plan.ChangeCount);
        foreach (var addition in plan.Added)
        {
            operations.Add(new SyncManifestEntry(
                SyncManifestOperation.Add,
                keyFormatter(addition.Key),
                DeltaManifest.SerializeValue(null, options),
                DeltaManifest.SerializeValue(addition.Desired, options),
                []));
        }

        foreach (var update in plan.Updated)
        {
            var deltaManifest = DeltaManifest.Create(
                update.Delta,
                typeof(TCurrent).FullName + " -> " + typeof(TDesired).FullName,
                options);
            operations.Add(new SyncManifestEntry(
                SyncManifestOperation.Update,
                keyFormatter(update.Key),
                DeltaManifest.SerializeValue(update.Current, options),
                DeltaManifest.SerializeValue(update.Desired, options),
                deltaManifest.Changes));
        }

        foreach (var removal in plan.Removed)
        {
            operations.Add(new SyncManifestEntry(
                SyncManifestOperation.Remove,
                keyFormatter(removal.Key),
                DeltaManifest.SerializeValue(removal.Current, options),
                DeltaManifest.SerializeValue(null, options),
                []));
        }

        var type = (typeof(TCurrent).FullName ?? typeof(TCurrent).Name)
            + " -> "
            + (typeof(TDesired).FullName ?? typeof(TDesired).Name);
        return new SyncManifestDocument(
            SyncManifestDocument.CurrentSchemaVersion,
            type,
            operations.AsReadOnly());
    }
}
