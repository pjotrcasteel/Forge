namespace Forge.Sync;

/// <summary>Validated immutable typed provenance graph with root-cause tracing.</summary>
public sealed class ProvenanceGraph<TNode, TKey>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TNode> _nodes;
    private readonly Dictionary<TKey, List<TKey>> _causesByEffect;
    private readonly Dictionary<TKey, int> _order;
    private readonly ProvenanceDefinition<TNode, TKey> _definition;

    private ProvenanceGraph(
        Dictionary<TKey, TNode> nodes,
        Dictionary<TKey, List<TKey>> causesByEffect,
        Dictionary<TKey, int> order,
        ProvenanceDefinition<TNode, TKey> definition)
    {
        _nodes = nodes;
        _causesByEffect = causesByEffect;
        _order = order;
        _definition = definition;
    }

    /// <summary>Creates a validated provenance graph.</summary>
    public static ProvenanceGraph<TNode, TKey> Create(
        IReadOnlyList<TNode> nodes,
        IReadOnlyList<ProvenanceEdge<TKey>> edges,
        ProvenanceDefinition<TNode, TKey> definition)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);
        ArgumentNullException.ThrowIfNull(definition);
        var byKey = new Dictionary<TKey, TNode>(nodes.Count, definition.KeyComparer);
        var order = new Dictionary<TKey, int>(nodes.Count, definition.KeyComparer);
        var causes = new Dictionary<TKey, List<TKey>>(nodes.Count, definition.KeyComparer);

        for (var index = 0; index < nodes.Count; index++)
        {
            var node = nodes[index];
            ArgumentNullException.ThrowIfNull(node);
            var key = definition.KeySelector(node);
            if (!byKey.TryAdd(key, node))
            {
                throw new DuplicateSyncKeyException(typeof(TNode), nameof(nodes), Convert.ToString(key) ?? string.Empty);
            }

            order.Add(key, index);
            causes.Add(key, []);
        }

        var seenCauses = byKey.Keys.ToDictionary(
            static key => key,
            _ => new HashSet<TKey>(definition.KeyComparer),
            definition.KeyComparer);
        foreach (var edge in edges)
        {
            if (!byKey.ContainsKey(edge.Cause) || !byKey.ContainsKey(edge.Effect))
            {
                throw new ArgumentException("Every provenance edge must reference nodes in the supplied graph.", nameof(edges));
            }

            if (seenCauses[edge.Effect].Add(edge.Cause))
            {
                causes[edge.Effect].Add(edge.Cause);
            }
        }

        var dependencyPlan = DependencyPlanner.Plan(
            nodes,
            definition.KeySelector,
            node => causes[definition.KeySelector(node)],
            definition.KeyComparer);
        if (dependencyPlan.HasCycles)
        {
            throw new ArgumentException("A provenance graph must be acyclic.", nameof(edges));
        }

        return new ProvenanceGraph<TNode, TKey>(byKey, causes, order, definition);
    }

    /// <summary>Traces all ancestors and one shortest deterministic path from every root cause to the target.</summary>
    public ProvenanceTrace<TNode, TKey> Trace(TKey targetKey)
    {
        if (!_nodes.TryGetValue(targetKey, out var target))
        {
            throw new KeyNotFoundException("The requested provenance target is not present in the graph.");
        }

        var distance = new Dictionary<TKey, int>(_definition.KeyComparer) { [targetKey] = 0 };
        var nextTowardTarget = new Dictionary<TKey, TKey>(_definition.KeyComparer);
        var queue = new Queue<TKey>();
        queue.Enqueue(targetKey);
        while (queue.Count != 0)
        {
            var effect = queue.Dequeue();
            foreach (var cause in _causesByEffect[effect])
            {
                if (distance.TryAdd(cause, distance[effect] + 1))
                {
                    nextTowardTarget.Add(cause, effect);
                    queue.Enqueue(cause);
                }
            }
        }

        var ancestorKeys = distance.Keys
            .Where(key => !_definition.KeyComparer.Equals(key, targetKey))
            .OrderBy(key => _order[key])
            .ToArray();
        var roots = ancestorKeys
            .Where(key => _causesByEffect[key].All(cause => !distance.ContainsKey(cause)))
            .ToArray();
        var paths = roots.Select(root => BuildShortestPath(root, targetKey, nextTowardTarget)).ToArray();
        var ancestors = ancestorKeys.Select(key => _nodes[key]).ToArray();
        return new ProvenanceTrace<TNode, TKey>(target, ancestors, paths);
    }

    private ProvenancePath<TNode, TKey> BuildShortestPath(
        TKey root,
        TKey target,
        IReadOnlyDictionary<TKey, TKey> nextTowardTarget)
    {
        var path = new List<TNode> { _nodes[root] };
        var current = root;
        while (!_definition.KeyComparer.Equals(current, target))
        {
            current = nextTowardTarget[current];
            path.Add(_nodes[current]);
        }

        return new ProvenancePath<TNode, TKey>(path);
    }
}
