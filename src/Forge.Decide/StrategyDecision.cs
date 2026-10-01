namespace Forge.Decide;

/// <summary>
/// Represents the deterministic result of selecting one applicable proposal from a typed strategy space.
/// </summary>
/// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed record StrategyDecision<TSpace, TPlan>
{
    internal StrategyDecision(StrategySpaceId spaceId, ApplicableStrategyCandidate<TPlan> selected, IReadOnlyList<StrategyCandidate<TPlan>> candidates)
    {
        SpaceId = spaceId;
        Selected = selected;
        Candidates = candidates;
    }

    /// <summary>
    /// Gets the strategy-space identifier.
    /// </summary>
    public StrategySpaceId SpaceId { get; }

    /// <summary>
    /// Gets the selected strategy and its proposed plan.
    /// </summary>
    public ApplicableStrategyCandidate<TPlan> Selected { get; }

    /// <summary>
    /// Gets the selected strategy identifier.
    /// </summary>
    public StrategyId SelectedStrategyId => Selected.StrategyId;

    /// <summary>
    /// Gets the selected application-owned plan.
    /// </summary>
    public TPlan Plan => Selected.Plan;

    /// <summary>
    /// Gets every evaluated candidate used to make the decision.
    /// </summary>
    public IReadOnlyList<StrategyCandidate<TPlan>> Candidates { get; }
}