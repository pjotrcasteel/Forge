using System.Text.Json;
using Forge.Delta;

namespace Forge.Sync;

/// <summary>Portable JSON-safe description of one Sync operation.</summary>
public sealed record SyncManifestEntry(
    SyncManifestOperation Operation,
    string Key,
    JsonElement Current,
    JsonElement Desired,
    IReadOnlyList<ChangeManifestEntry> Changes);
