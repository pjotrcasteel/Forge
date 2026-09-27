using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Ordered, deterministic rules for turning a cross-type structural Sync plan into application operations.
/// </summary>
public sealed class CrossSyncOperationRules<TCurrent, TDesired, TKey, TDelta, TOperation>
    where TKey : notnull
    where TDelta : IDelta
{
    private readonly List<UpdateRule> _updateRules = [];
    private readonly OperationDecision _addition;
    private readonly OperationDecision _removal;
    private readonly OperationDecision _fallbackUpdate;

    /// <summary>
    /// Creates an operation rule set with required defaults for add, remove and unmatched update operations.
    /// </summary>
    public CrossSyncOperationRules(
        TOperation addition,
        TOperation removal,
        TOperation fallbackUpdate)
    {
        _addition = new OperationDecision(addition, "operation.add", "Added state requires the configured add operation.");
        _removal = new OperationDecision(removal, "operation.remove", "Removed state requires the configured remove operation.");
        _fallbackUpdate = new OperationDecision(
            fallbackUpdate,
            "operation.update.default",
            "No more specific update rule matched.");
    }

    /// <summary>
    /// Adds an ordered update rule. The first matching rule wins.
    /// </summary>
    public CrossSyncOperationRules<TCurrent, TDesired, TKey, TDelta, TOperation> WhenUpdated(
        string code,
        string summary,
        Func<CrossSyncUpdate<TCurrent, TDesired, TKey, TDelta>, bool> predicate,
        TOperation operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentNullException.ThrowIfNull(predicate);
        _updateRules.Add(new UpdateRule(code, summary, predicate, operation));
        return this;
    }

    /// <summary>
    /// Classifies a plan and returns both typed operations and rule explanations.
    /// </summary>
    public OperationPlanningResult<CrossSyncOperationPlan<TCurrent, TDesired, TKey, TDelta, TOperation>, TKey> Plan(
        CrossSyncPlan<TCurrent, TDesired, TKey, TDelta> plan,
        IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var decisions = new Dictionary<TKey, OperationDecision>(comparer ?? EqualityComparer<TKey>.Default);
        var operations = CrossSyncOperationPlanner.Classify(
            plan,
            addition => Record(addition.Key, _addition, decisions),
            update => Record(update.Key, Select(update), decisions),
            removal => Record(removal.Key, _removal, decisions));

        var builder = new PlanExplanationBuilder<TKey>(SyncPlanExplainer.Explain(plan, comparer), comparer);
        foreach (var decision in decisions)
        {
            builder.AddReason(
                decision.Key,
                new PlanExplanationNode(
                    decision.Value.Code,
                    decision.Value.Summary,
                    after: decision.Value.Operation));
        }

        return new OperationPlanningResult<CrossSyncOperationPlan<TCurrent, TDesired, TKey, TDelta, TOperation>, TKey>(
            operations,
            builder.Build());
    }

    private OperationDecision Select(CrossSyncUpdate<TCurrent, TDesired, TKey, TDelta> update)
    {
        foreach (var rule in _updateRules)
        {
            if (rule.Predicate(update))
            {
                return new OperationDecision(rule.Operation, rule.Code, rule.Summary);
            }
        }

        return _fallbackUpdate;
    }

    private static TOperation Record(
        TKey key,
        OperationDecision decision,
        IDictionary<TKey, OperationDecision> decisions)
    {
        decisions.Add(key, decision);
        return decision.Operation;
    }

    private sealed record UpdateRule(
        string Code,
        string Summary,
        Func<CrossSyncUpdate<TCurrent, TDesired, TKey, TDelta>, bool> Predicate,
        TOperation Operation);

    private sealed record OperationDecision(TOperation Operation, string Code, string Summary);
}
