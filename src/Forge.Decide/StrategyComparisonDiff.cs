namespace Forge.Decide;

/// <summary>
/// Compares strategy evidence across two evaluations of the same strategy space.
/// </summary>
public static class StrategyComparisonDiff
{
    /// <summary>
    /// Compares two evaluations and reports candidate-level applicability, reason and plan changes.
    /// </summary>
    /// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
    /// <typeparam name="TPlan">Application-owned plan type.</typeparam>
    /// <param name="before">Earlier evaluation.</param>
    /// <param name="after">Later evaluation.</param>
    /// <param name="canonicalizePlan">Application-owned deterministic canonicalizer for proposed plans.</param>
    /// <returns>A comparison diff in stable candidate order.</returns>
    public static StrategyComparisonDiff<TSpace, TPlan> Between<TSpace, TPlan>(
        StrategyComparison<TSpace, TPlan> before,
        StrategyComparison<TSpace, TPlan> after,
        Func<TPlan, string> canonicalizePlan)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);
        ArgumentNullException.ThrowIfNull(canonicalizePlan);

        if (before.SpaceId != after.SpaceId)
        {
            throw new ArgumentException($"Cannot diff strategy spaces '{before.SpaceId}' and '{after.SpaceId}'.", nameof(after));
        }

        var afterById = after.Candidates.ToDictionary(candidate => candidate.StrategyId);
        var beforeIds = before.Candidates.Select(candidate => candidate.StrategyId).ToHashSet();
        var diffs = new List<StrategyCandidateDiff<TPlan>>(Math.Max(before.Candidates.Count, after.Candidates.Count));

        foreach (var beforeCandidate in before.Candidates)
        {
            if (!afterById.TryGetValue(beforeCandidate.StrategyId, out var afterCandidate))
            {
                diffs.Add(new StrategyCandidateDiff<TPlan>(
                    beforeCandidate.StrategyId,
                    beforeCandidate,
                    null,
                    StrategyCandidateChange.Removed));
                continue;
            }

            diffs.Add(new StrategyCandidateDiff<TPlan>(
                beforeCandidate.StrategyId,
                beforeCandidate,
                afterCandidate,
                CompareCandidate(beforeCandidate, afterCandidate, canonicalizePlan)));
        }

        foreach (var afterCandidate in after.Candidates.Where(candidate => !beforeIds.Contains(candidate.StrategyId)))
        {
            diffs.Add(new StrategyCandidateDiff<TPlan>(
                afterCandidate.StrategyId,
                null,
                afterCandidate,
                StrategyCandidateChange.Added));
        }

        return new StrategyComparisonDiff<TSpace, TPlan>(before.SpaceId, Array.AsReadOnly(diffs.ToArray()));
    }

    private static StrategyCandidateChange CompareCandidate<TPlan>(
        StrategyCandidate<TPlan> before,
        StrategyCandidate<TPlan> after,
        Func<TPlan, string> canonicalizePlan)
    {
        var changes = StrategyCandidateChange.None;

        if (before.IsApplicable != after.IsApplicable)
        {
            changes |= StrategyCandidateChange.ApplicabilityChanged;
        }

        if (!string.Equals(GetReason(before), GetReason(after), StringComparison.Ordinal))
        {
            changes |= StrategyCandidateChange.ReasonChanged;
        }

        if (before is ApplicableStrategyCandidate<TPlan> beforeApplicable &&
            after is ApplicableStrategyCandidate<TPlan> afterApplicable &&
            !string.Equals(canonicalizePlan(beforeApplicable.Plan), canonicalizePlan(afterApplicable.Plan), StringComparison.Ordinal))
        {
            changes |= StrategyCandidateChange.PlanChanged;
        }

        return changes;
    }

    private static string? GetReason<TPlan>(StrategyCandidate<TPlan> candidate) => candidate switch
    {
        ApplicableStrategyCandidate<TPlan> applicable => applicable.Reason,
        RejectedStrategyCandidate<TPlan> rejected => rejected.Reason,
        _ => throw new InvalidOperationException($"Unsupported candidate type '{candidate.GetType().FullName}'."),
    };
}