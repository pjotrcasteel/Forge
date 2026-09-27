namespace Forge.Sync;

/// <summary>Application-policy selection result with deterministic first-in-order tie handling.</summary>
public sealed class PlanSelectionResult<TAlternativeId, TPlan, TMetrics>
    where TAlternativeId : notnull
{
    public PlanSelectionResult(
        EvaluatedPlanAlternative<TAlternativeId, TPlan, TMetrics> selected,
        IReadOnlyList<EvaluatedPlanAlternative<TAlternativeId, TPlan, TMetrics>> equallyPreferred)
    {
        ArgumentNullException.ThrowIfNull(selected);
        ArgumentNullException.ThrowIfNull(equallyPreferred);
        Selected = selected;
        EquallyPreferred = Array.AsReadOnly(equallyPreferred.ToArray());
    }

    public EvaluatedPlanAlternative<TAlternativeId, TPlan, TMetrics> Selected { get; }
    public IReadOnlyList<EvaluatedPlanAlternative<TAlternativeId, TPlan, TMetrics>> EquallyPreferred { get; }
    public bool HasTie => EquallyPreferred.Count > 1;
}
