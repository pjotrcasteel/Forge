namespace Forge.Sync;

/// <summary>Mutable construction boundary used only while instantiating a strongly typed reusable plan template.</summary>
public sealed class PlanTemplateBuilder<TOperation, TKey>
    where TKey : notnull
{
    private readonly IEqualityComparer<TKey> _comparer;
    private readonly List<PlanTemplateOperation<TOperation, TKey>> _operations = [];
    private readonly List<DependencyEdge<TKey>> _dependencies = [];
    private readonly HashSet<TKey> _keys;

    public PlanTemplateBuilder(IEqualityComparer<TKey>? comparer = null)
    {
        _comparer = comparer ?? EqualityComparer<TKey>.Default;
        _keys = new HashSet<TKey>(_comparer);
    }

    public void AddOperation(TKey key, TOperation operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (!_keys.Add(key))
        {
            throw new DuplicateSyncKeyException(typeof(TOperation), nameof(operation), Convert.ToString(key) ?? string.Empty);
        }

        _operations.Add(new PlanTemplateOperation<TOperation, TKey>(key, operation));
    }

    public void AddDependency(TKey predecessor, TKey successor)
        => _dependencies.Add(new DependencyEdge<TKey>(predecessor, successor));

    public PlanTemplateInstance<TOperation, TKey> Build()
    {
        var predecessors = _keys.ToDictionary(key => key, _ => new HashSet<TKey>(_comparer), _comparer);
        foreach (var dependency in _dependencies)
        {
            if (!_keys.Contains(dependency.Predecessor) || !_keys.Contains(dependency.Successor))
            {
                throw new InvalidOperationException("Template dependencies must reference operations emitted by the same template instance.");
            }

            predecessors[dependency.Successor].Add(dependency.Predecessor);
        }

        var dependencyPlan = DependencyPlanner.Plan(
            _operations,
            static item => item.Key,
            item => predecessors[item.Key],
            _comparer);
        return new PlanTemplateInstance<TOperation, TKey>(_operations, _dependencies, dependencyPlan);
    }
}
