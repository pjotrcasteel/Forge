namespace Forge.Sync;

/// <summary>Names a portable plan so multiple heterogeneous plans can be composed.</summary>
public sealed record NamedSyncManifest(string Name, SyncManifestDocument Manifest);
