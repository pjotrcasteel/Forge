namespace Forge.Decide;

/// <summary>
/// Contains the side-effect-free proposals produced by a typed strategy space.
/// </summary>
/// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed record StrategyComparison<TSpace, TPlan>
{
    internal StrategyComparison(StrategySpaceId spaceId, IReadOnlyList<StrategyCandidate<TPlan>> candidates)
    {
        SpaceId = spaceId;
        Candidates = candidates;
    }

    /// <summary>
    /// Gets the strategy-space identifier.
    /// </summary>
    public StrategySpaceId SpaceId { get; }

    /// <summary>
    /// Gets every evaluated candidate in explicit registration order.
    /// </summary>
    public IReadOnlyList<StrategyCandidate<TPlan>> Candidates { get; }
}