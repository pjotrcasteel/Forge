using System.Text.Json;

namespace Forge.Delta;

/// <summary>Creates portable manifests from generated or custom Delta implementations.</summary>
public static class DeltaManifest
{
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

    public static JsonElement SerializeValue(object? value, JsonSerializerOptions? options)
    {
        return JsonSerializer.SerializeToElement(
            value,
            value?.GetType() ?? typeof(object),
            options);
    }
}
