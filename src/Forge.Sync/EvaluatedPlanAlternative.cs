namespace Forge.Sync;

/// <summary>One candidate plan paired with application-defined evaluation metrics.</summary>
public sealed record EvaluatedPlanAlternative<TAlternativeId, TPlan, TMetrics>(
    TAlternativeId Id,
    TPlan Plan,
    TMetrics Metrics)
    where TAlternativeId : notnull;
