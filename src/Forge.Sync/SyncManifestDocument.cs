using System.Text.Json;

namespace Forge.Sync;

/// <summary>Versioned portable representation of a structural reconciliation plan.</summary>
public sealed record SyncManifestDocument(
    int SchemaVersion,
    string ItemType,
    IReadOnlyList<SyncManifestEntry> Operations)
{
    public const int CurrentSchemaVersion = 1;

    public bool HasChanges => Operations.Count != 0;
    public int OperationCount => Operations.Count;

    public string ToJson(JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(this, options);

    public static SyncManifestDocument Parse(string json, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<SyncManifestDocument>(json, options)
            ?? throw new JsonException("The Sync manifest could not be deserialized.");
    }
}
