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

    /// <summary>
    /// Creates a portable diagnostic explanation without embedding the application-owned plan.
    /// </summary>
    /// <returns>A stable explanation of the selected, applicable and rejected candidates.</returns>
    public StrategyDecisionExplanation Explain()
    {
        var explanations = Candidates.Select(candidate => candidate switch
        {
            ApplicableStrategyCandidate<TPlan> applicable when applicable.StrategyId == SelectedStrategyId =>
                new StrategyCandidateExplanation(applicable.StrategyId, StrategyCandidateDisposition.Selected, applicable.Reason),
            ApplicableStrategyCandidate<TPlan> applicable =>
                new StrategyCandidateExplanation(applicable.StrategyId, StrategyCandidateDisposition.Applicable, applicable.Reason),
            RejectedStrategyCandidate<TPlan> rejected =>
                new StrategyCandidateExplanation(rejected.StrategyId, StrategyCandidateDisposition.Rejected, rejected.Reason),
            _ => throw new InvalidOperationException($"Unsupported candidate type '{candidate.GetType().FullName}'."),
        }).ToArray();

        return new StrategyDecisionExplanation(SpaceId, SelectedStrategyId, explanations);
    }
}