namespace Forge.Decide;

internal sealed class ExtremumStrategySelectionPolicy<TContext, TPlan, TValue> : IStrategySelectionPolicy<TContext, TPlan>
{
    private readonly Func<TContext, ApplicableStrategyCandidate<TPlan>, TValue> _valueSelector;
    private readonly IComparer<TValue> _comparer;
    private readonly bool _selectHighest;

    public ExtremumStrategySelectionPolicy(
        Func<TContext, ApplicableStrategyCandidate<TPlan>, TValue> valueSelector,
        IComparer<TValue>? comparer,
        bool selectHighest)
    {
        ArgumentNullException.ThrowIfNull(valueSelector);
        _valueSelector = valueSelector;
        _comparer = comparer ?? Comparer<TValue>.Default;
        _selectHighest = selectHighest;
    }

    public ValueTask<ApplicableStrategyCandidate<TPlan>> SelectAsync(
        TContext context,
        IReadOnlyList<ApplicableStrategyCandidate<TPlan>> candidates,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        cancellationToken.ThrowIfCancellationRequested();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("An extremum selection policy requires at least one applicable candidate.");
        }

        var selected = candidates[0];
        var selectedValue = _valueSelector(context, selected);
        var tied = new List<ApplicableStrategyCandidate<TPlan>> { selected };

        for (var index = 1; index < candidates.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var candidate = candidates[index];
            var candidateValue = _valueSelector(context, candidate);
            var comparison = _comparer.Compare(candidateValue, selectedValue);
            var isBetter = _selectHighest ? comparison > 0 : comparison < 0;

            if (isBetter)
            {
                selected = candidate;
                selectedValue = candidateValue;
                tied.Clear();
                tied.Add(candidate);
                continue;
            }

            if (comparison == 0)
            {
                tied.Add(candidate);
            }
        }

        if (tied.Count > 1)
        {
            throw new AmbiguousStrategyDecisionException(tied.Select(candidate => candidate.StrategyId).ToArray());
        }

        return ValueTask.FromResult(selected);
    }
}