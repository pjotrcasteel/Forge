namespace Forge.Sync;

/// <summary>Evaluates composable strongly typed readiness requirements without dispatching work.</summary>
public static class ReadinessEvaluator
{
    /// <summary>Evaluates every item in input order and accumulates all blocking reasons for each item.</summary>
    public static ReadinessPlan<TItem, TKey, TReason> Evaluate<TItem, TKey, TContext, TReason>(
        IReadOnlyList<TItem> items,
        Func<TItem, TKey> keySelector,
        TContext context,
        IReadinessRequirementProvider<TItem, TContext, TReason> requirementProvider)
        where TKey : notnull
        where TReason : notnull
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(requirementProvider);
        var runnable = new List<ReadyPlanItem<TItem, TKey>>(items.Count);
        var blocked = new List<BlockedPlanItem<TItem, TKey, TReason>>();

        foreach (var item in items)
        {
            ArgumentNullException.ThrowIfNull(item);
            List<TReason>? reasons = null;
            foreach (var requirement in requirementProvider.GetRequirements(item))
            {
                ArgumentNullException.ThrowIfNull(requirement);
                var evaluation = requirement.Evaluate(context);
                if (!evaluation.IsSatisfied)
                {
                    (reasons ??= []).Add(evaluation.Reason);
                }
            }

            var key = keySelector(item);
            if (reasons is null)
            {
                runnable.Add(new ReadyPlanItem<TItem, TKey>(key, item));
            }
            else
            {
                blocked.Add(new BlockedPlanItem<TItem, TKey, TReason>(key, item, reasons));
            }
        }

        return new ReadinessPlan<TItem, TKey, TReason>(runnable, blocked);
    }
}
