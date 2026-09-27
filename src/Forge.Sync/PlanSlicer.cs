namespace Forge.Sync;

/// <summary>Builds dependency-safe plan subsets without changing operation semantics.</summary>
public static class PlanSlicer
{
    /// <summary>Returns the selected items plus every in-plan predecessor they transitively require.</summary>
    public static PlanSlice<TItem, TKey> Slice<TItem, TKey, TSelector>(
        IReadOnlyList<TItem> items,
        Func<TItem, TKey> keySelector,
        Func<TItem, IEnumerable<TKey>> dependencySelector,
        TSelector selector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
        where TSelector : IPlanSliceSelector<TItem>
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(dependencySelector);
        ArgumentNullException.ThrowIfNull(selector);
        comparer ??= EqualityComparer<TKey>.Default;

        var byKey = new Dictionary<TKey, TItem>(items.Count, comparer);
        var orderedKeys = new TKey[items.Count];
        var directKeys = new HashSet<TKey>(comparer);
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            ArgumentNullException.ThrowIfNull(item);
            var key = keySelector(item);
            if (!byKey.TryAdd(key, item))
            {
                throw new DuplicateSyncKeyException(typeof(TItem), nameof(items), Convert.ToString(key) ?? string.Empty);
            }

            orderedKeys[index] = key;
            if (selector.Include(item))
            {
                directKeys.Add(key);
            }
        }

        var includedKeys = new HashSet<TKey>(directKeys, comparer);
        var stack = new Stack<TKey>(directKeys.Reverse());
        while (stack.Count != 0)
        {
            var key = stack.Pop();
            var item = byKey[key];
            foreach (var dependency in dependencySelector(item) ?? [])
            {
                if (!byKey.ContainsKey(dependency) || !includedKeys.Add(dependency))
                {
                    continue;
                }

                stack.Push(dependency);
            }
        }

        var included = new List<TItem>();
        var direct = new List<TItem>();
        var required = new List<TItem>();
        var excluded = new List<TItem>();
        foreach (var key in orderedKeys)
        {
            var item = byKey[key];
            if (!includedKeys.Contains(key))
            {
                excluded.Add(item);
                continue;
            }

            included.Add(item);
            if (directKeys.Contains(key))
            {
                direct.Add(item);
            }
            else
            {
                required.Add(item);
            }
        }

        return new PlanSlice<TItem, TKey>(included, direct, required, excluded);
    }
}
