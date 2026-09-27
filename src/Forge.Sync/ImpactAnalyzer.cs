namespace Forge.Sync;

/// <summary>
/// Performs deterministic breadth-first impact propagation over an application-supplied directed graph.
/// </summary>
public static class ImpactAnalyzer
{
    /// <summary>
    /// Calculates direct and transitive impact and records one shortest path to every affected item.
    /// </summary>
    public static ImpactAnalysis<TKey, TReason> Analyze<TKey, TReason>(
        IReadOnlyList<TKey> seeds,
        IReadOnlyList<ImpactEdge<TKey, TReason>> edges,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(seeds);
        ArgumentNullException.ThrowIfNull(edges);
        var effectiveComparer = comparer ?? EqualityComparer<TKey>.Default;
        var uniqueSeeds = new List<TKey>();
        var seedSet = new HashSet<TKey>(effectiveComparer);
        foreach (var seed in seeds)
        {
            if (seedSet.Add(seed))
            {
                uniqueSeeds.Add(seed);
            }
        }

        var outgoing = new Dictionary<TKey, List<ImpactEdge<TKey, TReason>>>(effectiveComparer);
        foreach (var edge in edges)
        {
            if (!outgoing.TryGetValue(edge.Source, out var list))
            {
                list = [];
                outgoing.Add(edge.Source, list);
            }
            list.Add(edge);
        }

        var paths = new Dictionary<TKey, ImpactPath<TKey, TReason>>(effectiveComparer);
        var visited = new HashSet<TKey>(seedSet, effectiveComparer);
        var queue = new Queue<PathState<TKey, TReason>>();
        foreach (var seed in uniqueSeeds)
        {
            queue.Enqueue(new PathState<TKey, TReason>(seed, seed, []));
        }

        var direct = new List<TKey>();
        var transitive = new List<TKey>();
        while (queue.Count != 0)
        {
            var state = queue.Dequeue();
            if (!outgoing.TryGetValue(state.Current, out var nextEdges))
            {
                continue;
            }

            foreach (var edge in nextEdges)
            {
                if (!visited.Add(edge.Target))
                {
                    continue;
                }

                var pathEdges = state.Edges.Append(edge).ToArray();
                var path = new ImpactPath<TKey, TReason>(state.Seed, edge.Target, pathEdges);
                paths.Add(edge.Target, path);
                if (path.Distance == 1)
                {
                    direct.Add(edge.Target);
                }
                else
                {
                    transitive.Add(edge.Target);
                }

                queue.Enqueue(new PathState<TKey, TReason>(state.Seed, edge.Target, pathEdges));
            }
        }

        return new ImpactAnalysis<TKey, TReason>(
            uniqueSeeds,
            direct,
            transitive,
            new System.Collections.ObjectModel.ReadOnlyDictionary<TKey, ImpactPath<TKey, TReason>>(paths));
    }

    private sealed record PathState<TKey, TReason>(
        TKey Seed,
        TKey Current,
        IReadOnlyList<ImpactEdge<TKey, TReason>> Edges);
}
