namespace Forge.Sync;

/// <summary>
/// Describes the structural outcome explained for one logical Sync item.
/// </summary>
public enum PlanExplanationAction
{
    Added,
    Updated,
    Removed,
    Unchanged,
    Preserved
}
