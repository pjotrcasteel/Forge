namespace Forge.Sync;

/// <summary>
/// Creates deterministic topological execution waves for an arbitrary keyed set of operations or resources.
/// </summary>
public static class DependencyPlanner
{
    public static DependencyPlan<T, TKey> Plan<T, TKey>(
        IReadOnlyList<T> items,
        Func<T, TKey> keySelector,
        Func<T, IEnumerable<TKey>> dependencySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(dependencySelector);
        comparer ??= EqualityComparer<TKey>.Default;

        var byKey = new Dictionary<TKey, T>(items.Count, comparer);
        var order = new Dictionary<TKey, int>(items.Count, comparer);
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            ArgumentNullException.ThrowIfNull(item);
            var key = keySelector(item);
            if (!byKey.TryAdd(key, item))
            {
                throw new DuplicateSyncKeyException(typeof(T), nameof(items), Convert.ToString(key) ?? string.Empty);
            }

            order.Add(key, index);
        }

        var indegree = new Dictionary<TKey, int>(items.Count, comparer);
        var dependents = new Dictionary<TKey, List<TKey>>(items.Count, comparer);
        foreach (var key in byKey.Keys)
        {
            indegree[key] = 0;
            dependents[key] = [];
        }

        foreach (var pair in byKey)
        {
            var seen = new HashSet<TKey>(comparer);
            foreach (var dependency in dependencySelector(pair.Value) ?? [])
            {
                if (!byKey.ContainsKey(dependency) || !seen.Add(dependency))
                {
                    continue;
                }

                indegree[pair.Key]++;
                dependents[dependency].Add(pair.Key);
            }
        }

        var remaining = new HashSet<TKey>(byKey.Keys, comparer);
        var waves = new List<DependencyWave<T>>();
        while (remaining.Count != 0)
        {
            var ready = remaining
                .Where(key => indegree[key] == 0)
                .OrderBy(key => order[key])
                .ToArray();
            if (ready.Length == 0)
            {
                break;
            }

            var waveItems = new T[ready.Length];
            for (var index = 0; index < ready.Length; index++)
            {
                var key = ready[index];
                waveItems[index] = byKey[key];
                remaining.Remove(key);
                foreach (var dependent in dependents[key])
                {
                    indegree[dependent]--;
                }
            }

            waves.Add(new DependencyWave<T>(waves.Count, waveItems));
        }

        var cycleKeys = remaining.OrderBy(key => order[key]).ToArray();
        var deleteWaves = waves
            .AsEnumerable()
            .Reverse()
            .Select((wave, index) => new DependencyWave<T>(index, wave.Items))
            .ToArray();

        return new DependencyPlan<T, TKey>(waves, deleteWaves, cycleKeys);
    }
}
