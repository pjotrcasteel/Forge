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

    public IReadOnlyList<ResolvedMergeValue> Values { get; }
    public IReadOnlyList<MergeConflict> UnresolvedConflicts { get; }
    public bool IsFullyResolved => UnresolvedConflicts.Count == 0;
}
