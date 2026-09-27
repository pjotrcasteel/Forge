namespace Forge.Sync;

/// <summary>Immutable evaluated alternatives that can be selected using application preference policy.</summary>
public sealed class EvaluatedPlanSet<TAlternativeId, TPlan, TMetrics>
    where TAlternativeId : notnull
{
    public EvaluatedPlanSet(IReadOnlyList<EvaluatedPlanAlternative<TAlternativeId, TPlan, TMetrics>> alternatives)
    {
        ArgumentNullException.ThrowIfNull(alternatives);
        Alternatives = Array.AsReadOnly(alternatives.ToArray());
    }

    public IReadOnlyList<EvaluatedPlanAlternative<TAlternativeId, TPlan, TMetrics>> Alternatives { get; }

    public PlanSelectionResult<TAlternativeId, TPlan, TMetrics> Select(IPlanPreference<TMetrics> preference)
    {
        ArgumentNullException.ThrowIfNull(preference);
        if (Alternatives.Count == 0)
        {
            throw new InvalidOperationException("At least one evaluated alternative is required for selection.");
        }

        var selected = Alternatives[0];
        for (var index = 1; index < Alternatives.Count; index++)
        {
            if (preference.Compare(Alternatives[index].Metrics, selected.Metrics) < 0)
            {
                selected = Alternatives[index];
            }
        }

        var equallyPreferred = Alternatives
            .Where(alternative => preference.Compare(alternative.Metrics, selected.Metrics) == 0)
            .ToArray();
        return new PlanSelectionResult<TAlternativeId, TPlan, TMetrics>(selected, equallyPreferred);
    }
}
