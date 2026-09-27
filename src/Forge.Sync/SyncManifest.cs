using System.Text.Json;
using Forge.Delta;

namespace Forge.Sync;

/// <summary>Creates versioned portable manifests from in-memory Sync plans.</summary>
public static class SyncManifest
{
    public static SyncManifestDocument Create<T, TKey, TDelta>(
        SyncPlan<T, TKey, TDelta> plan,
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
                Array.Empty<ChangeManifestEntry>()));
        }

        foreach (var update in plan.Updated)
        {
            var deltaManifest = DeltaManifest.Create(update.Delta, typeof(T).FullName ?? typeof(T).Name, options);
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
                Array.Empty<ChangeManifestEntry>()));
        }

        return new SyncManifestDocument(
            SyncManifestDocument.CurrentSchemaVersion,
            typeof(T).FullName ?? typeof(T).Name,
            operations.AsReadOnly());
    }
}
