namespace Forge.Sync;

/// <summary>Identifies the structural result emitted by streaming reconciliation.</summary>
public enum StreamingSyncChangeKind
{
    Added,
    Removed,
    Updated,
    Unchanged,
    Preserved
}
