using System.Text.Json;

namespace Forge.Delta;

/// <summary>Portable JSON representation of one property transition.</summary>
public sealed record ChangeManifestEntry(
    string Path,
    JsonElement Before,
    JsonElement After);
