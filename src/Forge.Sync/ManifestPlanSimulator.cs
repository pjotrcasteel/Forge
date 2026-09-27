using System.Text.Json;

namespace Forge.Sync;

/// <summary>
/// Simulates portable reconciliation manifests without performing infrastructure I/O or mutating domain objects.
/// </summary>
public static class ManifestPlanSimulator
{
    /// <summary>
    /// Applies a manifest to a portable current-state snapshot when all stale-plan preconditions still hold.
    /// </summary>
    public static ManifestSimulationResult Simulate(
        SyncManifestDocument manifest,
        IReadOnlyDictionary<string, JsonElement> currentState)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(currentState);
        var preconditions = ManifestPreconditions.Validate(manifest, currentState);
        var projected = Clone(currentState);
        if (!preconditions.IsSatisfied)
        {
            return new ManifestSimulationResult(
                preconditions,
                new System.Collections.ObjectModel.ReadOnlyDictionary<string, JsonElement>(projected),
                false);
        }

        foreach (var operation in manifest.Operations)
        {
            switch (operation.Operation)
            {
                case SyncManifestOperation.Add:
                case SyncManifestOperation.Update:
                    projected[operation.Key] = operation.Desired.Clone();
                    break;
                case SyncManifestOperation.Remove:
                    projected.Remove(operation.Key);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(manifest), operation.Operation, "Unknown Sync operation.");
            }
        }

        return new ManifestSimulationResult(
            preconditions,
            new System.Collections.ObjectModel.ReadOnlyDictionary<string, JsonElement>(projected),
            true);
    }

    private static Dictionary<string, JsonElement> Clone(IReadOnlyDictionary<string, JsonElement> state)
    {
        var result = new Dictionary<string, JsonElement>(state.Count, StringComparer.Ordinal);
        foreach (var item in state)
        {
            result.Add(item.Key, item.Value.Clone());
        }

        return result;
    }
}
