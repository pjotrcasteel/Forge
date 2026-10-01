namespace Forge.Decide;

/// <summary>
/// Evaluates only the strategies explicitly admitted to this typed candidate space.
/// </summary>
/// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
/// <typeparam name="TContext">Decision context type.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed class StrategySpace<TSpace, TContext, TPlan>
{
    private readonly IReadOnlyList<IStrategy<TContext, TPlan>> _strategies;
    private readonly IStrategySelectionPolicy<TContext, TPlan> _selectionPolicy;

    internal StrategySpace(
        StrategySpaceId id,
        IReadOnlyList<IStrategy<TContext, TPlan>> strategies,
        IStrategySelectionPolicy<TContext, TPlan> selectionPolicy)
    {
        Id = id;
        _strategies = strategies;
        _selectionPolicy = selectionPolicy;
    }

    /// <summary>
    /// Gets the stable strategy-space identifier.
    /// </summary>
    public StrategySpaceId Id { get; }

    /// <summary>
    /// Evaluates every admitted strategy without selecting or executing a plan.
    /// </summary>
    /// <param name="context">Decision context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The complete candidate comparison.</returns>
    public async ValueTask<StrategyComparison<TSpace, TPlan>> CompareAsync(TContext context, CancellationToken cancellationToken)
    {
        var candidates = new List<StrategyCandidate<TPlan>>(_strategies.Count);

        foreach (var strategy in _strategies)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var proposal = await strategy.ProposeAsync(context, cancellationToken).ConfigureAwait(false);

            candidates.Add(proposal switch
            {
                ApplicableStrategyProposal<TPlan> applicable =>
                    new ApplicableStrategyCandidate<TPlan>(strategy.Id, applicable.Plan, applicable.Reason),
                InapplicableStrategyProposal<TPlan> rejected =>
                    new RejectedStrategyCandidate<TPlan>(strategy.Id, rejected.Reason),
                null => throw new InvalidOperationException($"Strategy '{strategy.Id}' returned a null proposal."),
                _ => throw new InvalidOperationException($"Strategy '{strategy.Id}' returned an unsupported proposal type '{proposal.GetType().FullName}'."),
            });
        }

        return new StrategyComparison<TSpace, TPlan>(Id, candidates.AsReadOnly());
    }

    /// <summary>
    /// Evaluates the admitted strategies and deterministically selects one applicable proposal.
    /// </summary>
    /// <param name="context">Decision context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The selected decision and complete candidate evidence.</returns>
    public async ValueTask<StrategyDecision<TSpace, TPlan>> DecideAsync(TContext context, CancellationToken cancellationToken)
    {
        var comparison = await CompareAsync(context, cancellationToken).ConfigureAwait(false);
        var applicable = comparison.Candidates.OfType<ApplicableStrategyCandidate<TPlan>>().ToArray();

        if (applicable.Length == 0)
        {
            throw new NoApplicableStrategyException(Id);
        }

        var selected = await _selectionPolicy.SelectAsync(context, Array.AsReadOnly(applicable), cancellationToken).ConfigureAwait(false);

        if (!applicable.Any(candidate => ReferenceEquals(candidate, selected)))
        {
            throw new InvalidStrategySelectionException(selected.StrategyId);
        }

        return new StrategyDecision<TSpace, TPlan>(Id, selected, comparison.Candidates);
    }
}