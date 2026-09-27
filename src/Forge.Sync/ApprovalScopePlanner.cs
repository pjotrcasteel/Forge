namespace Forge.Sync;

/// <summary>Partitions typed operations into approval scopes and derives dependency-safe scope ordering.</summary>
public static class ApprovalScopePlanner
{
    public static ApprovalScopePlan<TScope, TOperation> Plan<TOperation, TKey, TScope>(
        IReadOnlyList<TOperation> operations,
        Func<TOperation, TKey> operationKeySelector,
        IReadOnlyList<DependencyEdge<TKey>> dependencies,
        IApprovalScopeSelector<TOperation, TScope> scopeSelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TScope>? scopeComparer = null)
        where TKey : notnull
        where TScope : notnull
    {
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(operationKeySelector);
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(scopeSelector);
        keyComparer ??= EqualityComparer<TKey>.Default;
        scopeComparer ??= EqualityComparer<TScope>.Default;

        var scopeByOperation = new Dictionary<TKey, TScope>(operations.Count, keyComparer);
        var grouped = new Dictionary<TScope, List<TOperation>>(scopeComparer);
        var scopeOrder = new List<TScope>();
        foreach (var operation in operations)
        {
            ArgumentNullException.ThrowIfNull(operation);
            var key = operationKeySelector(operation);
            var scope = scopeSelector.Select(operation);
            if (!scopeByOperation.TryAdd(key, scope))
            {
                throw new DuplicateSyncKeyException(typeof(TOperation), nameof(operations), Convert.ToString(key) ?? string.Empty);
            }

            if (!grouped.TryGetValue(scope, out var list))
            {
                list = [];
                grouped.Add(scope, list);
                scopeOrder.Add(scope);
            }

            list.Add(operation);
        }

        var predecessors = scopeOrder.ToDictionary(
            static scope => scope,
            _ => new HashSet<TScope>(scopeComparer),
            scopeComparer);
        var scopeDependencies = new List<ApprovalScopeDependency<TScope>>();
        foreach (var dependency in dependencies)
        {
            if (!scopeByOperation.TryGetValue(dependency.Predecessor, out var predecessorScope)
                || !scopeByOperation.TryGetValue(dependency.Successor, out var successorScope))
            {
                throw new ArgumentException("Approval dependencies must reference operations in the supplied plan.", nameof(dependencies));
            }

            if (scopeComparer.Equals(predecessorScope, successorScope)
                || !predecessors[successorScope].Add(predecessorScope))
            {
                continue;
            }

            scopeDependencies.Add(new ApprovalScopeDependency<TScope>(predecessorScope, successorScope));
        }

        var groups = scopeOrder
            .Select(scope => new ApprovalScopeGroup<TScope, TOperation>(scope, grouped[scope]))
            .ToArray();
        var dependencyPlan = DependencyPlanner.Plan(
            groups,
            static group => group.Scope,
            group => predecessors[group.Scope],
            scopeComparer);
        return new ApprovalScopePlan<TScope, TOperation>(groups, scopeDependencies, dependencyPlan);
    }
}
