namespace Forge.Delta;

/// <summary>
/// Describes a three-way merge conflict where current and desired state both changed the same semantic property differently.
/// </summary>
public sealed record MergeConflict(
    string Path,
    object? Baseline,
    object? Current,
    object? Desired);
