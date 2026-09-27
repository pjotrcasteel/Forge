using System.Text.Json;

namespace Forge.Delta;

/// <summary>Versioned portable representation of a Delta.</summary>
public sealed record DeltaManifestDocument(
    int SchemaVersion,
    string Type,
    IReadOnlyList<ChangeManifestEntry> Changes)
{
    public const int CurrentSchemaVersion = 1;

    public string ToJson(JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(this, options);

    public static DeltaManifestDocument Parse(string json, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<DeltaManifestDocument>(json, options)
            ?? throw new JsonException("The Delta manifest could not be deserialized.");
    }
}
