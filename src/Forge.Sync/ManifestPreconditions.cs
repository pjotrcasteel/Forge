using System.Text.Json;
using Forge.Delta;

namespace Forge.Sync;

/// <summary>Validates that current state still matches the state captured when a portable Sync plan was created.</summary>
public static class ManifestPreconditions
{
    public static ManifestPreconditionResult Validate(
        SyncManifestDocument manifest,
        IReadOnlyDictionary<string, JsonElement> currentState)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(currentState);
        var failures = new List<ManifestPreconditionFailure>();
        foreach (var operation in manifest.Operations)
        {
            var exists = currentState.TryGetValue(operation.Key, out var actual);
            if (operation.Operation == SyncManifestOperation.Add)
            {
                if (exists)
                {
                    failures.Add(new ManifestPreconditionFailure(
                        operation.Key,
                        ManifestPreconditionFailureReason.ExpectedAbsent,
                        DeltaManifest.SerializeValue(null, null),
                        actual));
                }

                continue;
            }

            if (!exists)
            {
                failures.Add(new ManifestPreconditionFailure(
                    operation.Key,
                    ManifestPreconditionFailureReason.ExpectedPresent,
                    operation.Current,
                    DeltaManifest.SerializeValue(null, null)));
                continue;
            }

            if (!PortableJsonComparer.AreEquivalent(operation.Current, actual))
            {
                failures.Add(new ManifestPreconditionFailure(
                    operation.Key,
                    ManifestPreconditionFailureReason.CurrentStateChanged,
                    operation.Current,
                    actual));
            }
        }

        return new ManifestPreconditionResult(failures);
    }
}
