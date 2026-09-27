using System.Text.Json;

namespace Forge.Delta;

/// <summary>Versioned portable representation of a Delta.</summary>
public sealed record DeltaManifestDocument(
    int SchemaVersion,
    string Type,
    IReadOnlyList<ChangeManifestEntry> Changes)
{
    /// <summary>
    /// Gets the schema version emitted by this Forge version.
    /// </summary>
    public const int CurrentSchemaVersion = 1;

    /// <summary>
    /// Serializes this manifest to JSON.
    /// </summary>
    /// <param name="options">Optional JSON serialization options.</param>
    /// <returns>The serialized manifest.</returns>
    public string ToJson(JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(this, options);

    /// <summary>
    /// Parses a portable delta manifest from JSON.
    /// </summary>
    /// <param name="json">The serialized manifest.</param>
    /// <param name="options">Optional JSON serialization options.</param>
    /// <returns>The parsed manifest.</returns>
    public static DeltaManifestDocument Parse(string json, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<DeltaManifestDocument>(json, options)
            ?? throw new JsonException("The Delta manifest could not be deserialized.");
    }
}
