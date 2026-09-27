namespace Forge.Sync;

/// <summary>Composes multiple typed topology results into one validated application transition.</summary>
public static class CompositeTopologyPlanner
{
    public static CompositeTopologyPlan<TContext, TOperation, TKey, TViolation> Compose<TContext, TOperation, TKey, TViolation>(
        TContext context,
        IReadOnlyList<TOperation> operations,
        Func<TOperation, TKey> keySelector,
        IReadOnlyList<DependencyEdge<TKey>> dependencies,
        IReadOnlyList<ICompositeTopologyInvariant<TContext, TViolation>> invariants,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(invariants);
        comparer ??= EqualityComparer<TKey>.Default;

        var byKey = new Dictionary<TKey, TOperation>(operations.Count, comparer);
        var predecessors = new Dictionary<TKey, HashSet<TKey>>(operations.Count, comparer);
        foreach (var operation in operations)
        {
            ArgumentNullException.ThrowIfNull(operation);
            var key = keySelector(operation);
            if (!byKey.TryAdd(key, operation))
            {
                throw new DuplicateSyncKeyException(typeof(TOperation), nameof(operations), Convert.ToString(key) ?? string.Empty);
            }

            predecessors.Add(key, new HashSet<TKey>(comparer));
        }

        foreach (var dependency in dependencies)
        {
            if (!byKey.ContainsKey(dependency.Predecessor) || !byKey.ContainsKey(dependency.Successor))
            {
                throw new ArgumentException("Composite dependencies must reference operations in the composed transition.", nameof(dependencies));
            }

            predecessors[dependency.Successor].Add(dependency.Predecessor);
        }

        var dependencyPlan = DependencyPlanner.Plan(
            operations,
            keySelector,
            operation => predecessors[keySelector(operation)],
            comparer);
        var violations = new List<TViolation>();
        foreach (var invariant in invariants)
        {
            ArgumentNullException.ThrowIfNull(invariant);
            var found = invariant.Validate(context);
            ArgumentNullException.ThrowIfNull(found);
            violations.AddRange(found);
        }

        return new CompositeTopologyPlan<TContext, TOperation, TKey, TViolation>(
            context,
            operations,
            dependencies,
            dependencyPlan,
            violations);
    }
}
