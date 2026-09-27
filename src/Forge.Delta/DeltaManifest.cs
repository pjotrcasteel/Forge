using System.Text.Json;

namespace Forge.Delta;

/// <summary>Creates portable manifests from generated or custom Delta implementations.</summary>
public static class DeltaManifest
{
    /// <summary>
    /// Creates a portable manifest from a semantic delta.
    /// </summary>
    /// <param name="delta">The delta whose changes should be serialized.</param>
    /// <param name="type">The logical type identifier stored in the manifest.</param>
    /// <param name="options">Optional JSON serialization options.</param>
    /// <returns>A portable delta manifest.</returns>
    public static DeltaManifestDocument Create(
        IDelta delta,
        string type,
        JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(delta);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        var entries = delta.Changes
            .Select(change => new ChangeManifestEntry(
                change.Path,
                SerializeValue(change.Before, options),
                SerializeValue(change.After, options)))
            .ToArray();
        return new DeltaManifestDocument(
            DeltaManifestDocument.CurrentSchemaVersion,
            type,
            entries);
    }

    /// <summary>
    /// Serializes one semantic value into its portable JSON representation.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">Optional JSON serialization options.</param>
    /// <returns>The serialized JSON element.</returns>
    public static JsonElement SerializeValue(object? value, JsonSerializerOptions? options)
    {
        return JsonSerializer.SerializeToElement(
            value,
            value?.GetType() ?? typeof(object),
            options);
    }
}
