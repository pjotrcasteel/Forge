namespace Forge.Delta;

/// <summary>
/// Describes one changed property without losing its original path.
/// </summary>
/// <param name="Path">The property path.</param>
/// <param name="Before">The value before the change.</param>
/// <param name="After">The value after the change.</param>
[System.Diagnostics.DebuggerDisplay("{Path,nq}: {Before} -> {After}")]
public sealed record PropertyChange(
    string Path,
    object? Before,
    object? After);