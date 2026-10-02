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
        Candidates = Array.AsReadOnly(candidates.ToArray());
        ApplicableCandidates = Array.AsReadOnly(Candidates.OfType<ApplicableStrategyCandidate<TPlan>>().ToArray());
        RejectedCandidates = Array.AsReadOnly(Candidates.OfType<RejectedStrategyCandidate<TPlan>>().ToArray());
    }

    /// <summary>
    /// Gets the strategy-space identifier.
    /// </summary>
    public StrategySpaceId SpaceId { get; }

    /// <summary>
    /// Gets every evaluated candidate in explicit registration order.
    /// </summary>
    public IReadOnlyList<StrategyCandidate<TPlan>> Candidates { get; }

    /// <summary>
    /// Gets the applicable proposals in explicit registration order.
    /// </summary>
    public IReadOnlyList<ApplicableStrategyCandidate<TPlan>> ApplicableCandidates { get; }

    /// <summary>
    /// Gets the rejected candidates in explicit registration order.
    /// </summary>
    public IReadOnlyList<RejectedStrategyCandidate<TPlan>> RejectedCandidates { get; }

    /// <summary>
    /// Selects one proposal from this already-evaluated comparison without re-running strategies.
    /// </summary>
    /// <typeparam name="TContext">Decision context type.</typeparam>
    /// <param name="context">Decision context supplied to the selection policy.</param>
    /// <param name="selectionPolicy">Policy used to select an applicable proposal.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strategy decision based on this exact comparison.</returns>
    public async ValueTask<StrategyDecision<TSpace, TPlan>> DecideAsync<TContext>(
        TContext context,
        IStrategySelectionPolicy<TContext, TPlan> selectionPolicy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(selectionPolicy);
        cancellationToken.ThrowIfCancellationRequested();

        if (ApplicableCandidates.Count == 0)
        {
            throw new NoApplicableStrategyException(SpaceId);
        }

        var selected = await selectionPolicy.SelectAsync(context, ApplicableCandidates, cancellationToken).ConfigureAwait(false);

        if (selected is null || !ApplicableCandidates.Any(candidate => ReferenceEquals(candidate, selected)))
        {
            throw new InvalidStrategySelectionException(selected?.StrategyId);
        }

        return new StrategyDecision<TSpace, TPlan>(SpaceId, selected, Candidates);
    }

    /// <summary>
    /// Applies a production and shadow policy to the same evaluated candidates.
    /// </summary>
    /// <typeparam name="TContext">Decision context type.</typeparam>
    /// <param name="context">Decision context supplied to both policies.</param>
    /// <param name="productionPolicy">Policy whose decision represents current behavior.</param>
    /// <param name="shadowPolicy">Policy evaluated for comparison only.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Production and shadow decisions derived from the same candidate evidence.</returns>
    public async ValueTask<StrategyShadowDecision<TSpace, TPlan>> DecideWithShadowAsync<TContext>(
        TContext context,
        IStrategySelectionPolicy<TContext, TPlan> productionPolicy,
        IStrategySelectionPolicy<TContext, TPlan> shadowPolicy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(productionPolicy);
        ArgumentNullException.ThrowIfNull(shadowPolicy);

        var production = await DecideAsync(context, productionPolicy, cancellationToken).ConfigureAwait(false);
        var shadow = await DecideAsync(context, shadowPolicy, cancellationToken).ConfigureAwait(false);

        return new StrategyShadowDecision<TSpace, TPlan>(production, shadow);
    }
}