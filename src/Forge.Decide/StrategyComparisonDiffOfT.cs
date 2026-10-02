namespace Forge.Decide;

/// <summary>
/// Represents candidate evidence changes across two evaluations of one strategy space.
/// </summary>
/// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed record StrategyComparisonDiff<TSpace, TPlan>
{
    internal StrategyComparisonDiff(StrategySpaceId spaceId, IReadOnlyList<StrategyCandidateDiff<TPlan>> candidates)
    {
        SpaceId = spaceId;
        Candidates = candidates;
    }

    /// <summary>
    /// Gets the compared strategy-space identifier.
    /// </summary>
    public StrategySpaceId SpaceId { get; }

    /// <summary>
    /// Gets candidate diffs in stable order: earlier candidates first, followed by later additions.
    /// </summary>
    public IReadOnlyList<StrategyCandidateDiff<TPlan>> Candidates { get; }

    /// <summary>
    /// Gets whether any candidate evidence changed.
    /// </summary>
    public bool HasChanges => Candidates.Any(candidate => candidate.HasChanges);
}