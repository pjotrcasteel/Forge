namespace Forge.Delta;

/// <summary>
/// Resolves generated three-way merge analysis into an explicit semantic patch without mutating domain objects.
/// </summary>
public static class MergeResolver
{
    /// <summary>
    /// Resolves non-conflicting branch changes automatically and applies explicit policies to conflicts.
    /// </summary>
    public static MergeResolutionResult Resolve<TDelta>(
        MergeAnalysis<TDelta> analysis,
        MergeResolutionPolicy? policy = null)
        where TDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(analysis);
        var conflictByPath = analysis.Conflicts.ToDictionary(conflict => conflict.Path, StringComparer.Ordinal);
        var currentByPath = analysis.CurrentDelta.Changes.ToDictionary(change => change.Path, StringComparer.Ordinal);
        var desiredByPath = analysis.DesiredDelta.Changes.ToDictionary(change => change.Path, StringComparer.Ordinal);
        var paths = currentByPath.Keys
            .Concat(desiredByPath.Keys)
            .Concat(conflictByPath.Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        var values = new List<ResolvedMergeValue>(paths.Length);
        var unresolved = new List<MergeConflict>();

        foreach (var path in paths)
        {
            if (conflictByPath.TryGetValue(path, out var conflict))
            {
                if (policy is not null && policy.TryResolve(conflict, out var resolved) && resolved is not null)
                {
                    values.Add(resolved);
                }
                else
                {
                    unresolved.Add(conflict);
                }
                continue;
            }

            var hasCurrent = currentByPath.TryGetValue(path, out var current);
            var hasDesired = desiredByPath.TryGetValue(path, out var desired);
            var value = hasDesired ? desired!.After : current!.After;
            values.Add(new ResolvedMergeValue(path, value, MergeResolutionSource.NonConflicting));
        }

        return new MergeResolutionResult(values, unresolved);
    }
}
