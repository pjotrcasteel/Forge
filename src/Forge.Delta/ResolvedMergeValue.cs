namespace Forge.Delta;

/// <summary>
/// One path in a resolved semantic three-way merge patch.
/// </summary>
public sealed record ResolvedMergeValue(
    string Path,
    object? Value,
    MergeResolutionSource Source);
