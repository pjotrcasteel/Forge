namespace Forge.Sync;

/// <summary>Immutable set of typed valid plan alternatives.</summary>
public sealed class PlanAlternativeSet<TAlternativeId, TPlan>
    where TAlternativeId : notnull
{
    public PlanAlternativeSet(
        IReadOnlyList<PlanAlternative<TAlternativeId, TPlan>> alternatives,
        IEqualityComparer<TAlternativeId>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(alternatives);
        var seen = new HashSet<TAlternativeId>(comparer ?? EqualityComparer<TAlternativeId>.Default);
        foreach (var alternative in alternatives)
        {
            ArgumentNullException.ThrowIfNull(alternative);
            if (!seen.Add(alternative.Id))
            {
                throw new ArgumentException("Plan alternative identities must be unique.", nameof(alternatives));
            }
        }

        Alternatives = Array.AsReadOnly(alternatives.ToArray());
    }

    public IReadOnlyList<PlanAlternative<TAlternativeId, TPlan>> Alternatives { get; }

    public EvaluatedPlanSet<TAlternativeId, TPlan, TMetrics> Evaluate<TMetrics>(IPlanEvaluator<TPlan, TMetrics> evaluator)
    {
        ArgumentNullException.ThrowIfNull(evaluator);
        var evaluated = Alternatives
            .Select(alternative => new EvaluatedPlanAlternative<TAlternativeId, TPlan, TMetrics>(
                alternative.Id,
                alternative.Plan,
                evaluator.Evaluate(alternative.Plan)))
            .ToArray();
        return new EvaluatedPlanSet<TAlternativeId, TPlan, TMetrics>(evaluated);
    }
}
