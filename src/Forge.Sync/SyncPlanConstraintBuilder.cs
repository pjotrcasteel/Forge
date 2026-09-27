using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Convenience constraints for same-type Sync plans while retaining access to arbitrary typed rules.
/// </summary>
public sealed class SyncPlanConstraintBuilder<T, TKey, TDelta>
    where TKey : notnull
    where TDelta : IDelta
{
    private readonly PlanConstraintSet<SyncPlan<T, TKey, TDelta>> _constraints = new();

    /// <summary>
    /// Limits the total number of add/update/remove operations.
    /// </summary>
    public SyncPlanConstraintBuilder<T, TKey, TDelta> MaximumChanges(int maximum)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maximum);
        _constraints.Require(
            "sync.maximum-changes",
            $"The plan may contain at most {maximum} state-changing operations.",
            plan => plan.ChangeCount <= maximum);
        return this;
    }

    /// <summary>
    /// Rejects any plan that contains removals.
    /// </summary>
    public SyncPlanConstraintBuilder<T, TKey, TDelta> RequireNoRemovals()
    {
        _constraints.Require(
            "sync.removals-forbidden",
            "The plan must not remove logical items.",
            plan => plan.Removed.Count == 0);
        return this;
    }

    /// <summary>
    /// Prevents one protected logical key from being removed.
    /// </summary>
    public SyncPlanConstraintBuilder<T, TKey, TDelta> ProtectKey(
        TKey key,
        IEqualityComparer<TKey>? comparer = null)
    {
        var effectiveComparer = comparer ?? EqualityComparer<TKey>.Default;
        _constraints.Require(
            "sync.protected-key",
            "A protected logical item must not be removed.",
            plan => !plan.Removed.Any(removal => effectiveComparer.Equals(removal.Key, key)),
            Convert.ToString(key, System.Globalization.CultureInfo.InvariantCulture));
        return this;
    }

    /// <summary>
    /// Adds an arbitrary strongly typed application constraint.
    /// </summary>
    public SyncPlanConstraintBuilder<T, TKey, TDelta> Require(
        string code,
        string message,
        Func<SyncPlan<T, TKey, TDelta>, bool> predicate,
        string? path = null)
    {
        _constraints.Require(code, message, predicate, path);
        return this;
    }

    /// <summary>
    /// Validates all configured constraints.
    /// </summary>
    public PlanConstraintValidation Validate(SyncPlan<T, TKey, TDelta> plan)
        => _constraints.Validate(plan);
}
