namespace Forge.Delta;

/// <summary>
/// Identifies where a resolved three-way merge value came from.
/// </summary>
public enum MergeResolutionSource
{
    Current,
    Desired,
    Baseline,
    Custom,
    NonConflicting
}
