namespace Forge.Delta;

/// <summary>
/// Immutable semantic merge patch plus conflicts for which no policy supplied a resolution.
/// </summary>
public sealed class MergeResolutionResult
{
    /// <summary>
    /// Creates a merge-resolution result.
    /// </summary>
    public MergeResolutionResult(
        IReadOnlyList<ResolvedMergeValue> values,
        IReadOnlyList<MergeConflict> unresolvedConflicts)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(unresolvedConflicts);
        Values = Array.AsReadOnly(values.ToArray());
        UnresolvedConflicts = Array.AsReadOnly(unresolvedConflicts.ToArray());
    }

    /// <summary>
    /// Gets the resolved semantic values that form the merge patch.
    /// </summary>
    public IReadOnlyList<ResolvedMergeValue> Values { get; }

    /// <summary>
    /// Gets conflicts for which no resolution policy supplied a value.
    /// </summary>
    public IReadOnlyList<MergeConflict> UnresolvedConflicts { get; }

    /// <summary>
    /// Gets whether every semantic conflict has been resolved.
    /// </summary>
    public bool IsFullyResolved => UnresolvedConflicts.Count == 0;
}
