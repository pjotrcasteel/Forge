using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Reconciles collections whose current and desired item types differ but share logical identity.
/// </summary>
public static class CrossSync
{
    /// <summary>
    /// Reconciles current and desired state without reflection or I/O.
    /// </summary>
    public static CrossSyncPlan<TCurrent, TDesired, TKey, TDelta> Plan<TCurrent, TDesired, TKey, TDelta>(
        IReadOnlyList<TCurrent> current,
        IReadOnlyList<TDesired> desired,
        CrossSyncDefinition<TCurrent, TDesired, TKey, TDelta> definition)
        where TKey : notnull
        where TDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(desired);
        ArgumentNullException.ThrowIfNull(definition);

        var currentByKey = Index(
            current,
            definition.CurrentKeySelector,
            definition.KeyComparer,
            nameof(current));
        var desiredKeys = new HashSet<TKey>(desired.Count, definition.KeyComparer);
        var added = new List<CrossSyncAddition<TDesired, TKey>>();
        var removed = new List<CrossSyncRemoval<TCurrent, TKey>>();
        var updated = new List<CrossSyncUpdate<TCurrent, TDesired, TKey, TDelta>>();
        var unchanged = new List<CrossSyncUnchanged<TCurrent, TDesired, TKey>>();
        var preserved = new List<CrossSyncPreserved<TCurrent, TKey>>();

        foreach (var desiredItem in desired)
        {
            ArgumentNullException.ThrowIfNull(desiredItem);
            var key = definition.DesiredKeySelector(desiredItem);
            if (!desiredKeys.Add(key))
            {
                throw new DuplicateSyncKeyException(typeof(TDesired), nameof(desired), Convert.ToString(key) ?? string.Empty);
            }

            if (!currentByKey.TryGetValue(key, out var currentItem))
            {
                added.Add(new CrossSyncAddition<TDesired, TKey>(key, desiredItem));
                continue;
            }

            if (definition.AreEquivalent(currentItem, desiredItem))
            {
                unchanged.Add(new CrossSyncUnchanged<TCurrent, TDesired, TKey>(key, currentItem, desiredItem));
                continue;
            }

            updated.Add(new CrossSyncUpdate<TCurrent, TDesired, TKey, TDelta>(
                key,
                currentItem,
                desiredItem,
                definition.DeltaFactory(currentItem, desiredItem)));
        }

        foreach (var currentItem in current)
        {
            ArgumentNullException.ThrowIfNull(currentItem);
            var key = definition.CurrentKeySelector(currentItem);
            if (desiredKeys.Contains(key))
            {
                continue;
            }

            if (definition.Mode == SyncMode.Upsert)
            {
                preserved.Add(new CrossSyncPreserved<TCurrent, TKey>(key, currentItem));
            }
            else
            {
                removed.Add(new CrossSyncRemoval<TCurrent, TKey>(key, currentItem));
            }
        }

        return new CrossSyncPlan<TCurrent, TDesired, TKey, TDelta>(
            added,
            removed,
            updated,
            unchanged,
            preserved);
    }

    /// <summary>
    /// Reconciles current and desired state using canonical identity plus ordered fallback identity keys.
    /// </summary>
    public static CrossSyncPlan<TCurrent, TDesired, TKey, TDelta> Plan<TCurrent, TDesired, TKey, TDelta>(
        IReadOnlyList<TCurrent> current,
        IReadOnlyList<TDesired> desired,
        CrossSyncMatchDefinition<TCurrent, TDesired, TKey, TDelta> definition)
        where TKey : notnull
        where TDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(desired);
        ArgumentNullException.ThrowIfNull(definition);

        var currentIdentities = new SyncIdentity<TKey>[current.Count];
        var currentByIdentityKey = new Dictionary<TKey, List<int>>(definition.KeyComparer);
        for (var index = 0; index < current.Count; index++)
        {
            var currentItem = current[index];
            ArgumentNullException.ThrowIfNull(currentItem);
            var identity = definition.CurrentIdentitySelector(currentItem);
            ArgumentNullException.ThrowIfNull(identity);
            currentIdentities[index] = identity;
            IndexIdentity(currentByIdentityKey, identity, index, definition.KeyComparer);
        }

        var matchedCurrent = new bool[current.Count];
        var desiredCanonicalKeys = new HashSet<TKey>(definition.KeyComparer);
        var added = new List<CrossSyncAddition<TDesired, TKey>>();
        var removed = new List<CrossSyncRemoval<TCurrent, TKey>>();
        var updated = new List<CrossSyncUpdate<TCurrent, TDesired, TKey, TDelta>>();
        var unchanged = new List<CrossSyncUnchanged<TCurrent, TDesired, TKey>>();
        var preserved = new List<CrossSyncPreserved<TCurrent, TKey>>();

        foreach (var desiredItem in desired)
        {
            ArgumentNullException.ThrowIfNull(desiredItem);
            var desiredIdentity = definition.DesiredIdentitySelector(desiredItem);
            ArgumentNullException.ThrowIfNull(desiredIdentity);
            var desiredKey = desiredIdentity.CanonicalKey;
            if (!desiredCanonicalKeys.Add(desiredKey))
            {
                throw new DuplicateSyncKeyException(typeof(TDesired), nameof(desired), Convert.ToString(desiredKey) ?? string.Empty);
            }

            var currentIndex = FindCurrentIndex(
                currentByIdentityKey,
                desiredIdentity,
                definition.KeyComparer,
                typeof(TCurrent));
            if (currentIndex is null)
            {
                added.Add(new CrossSyncAddition<TDesired, TKey>(desiredKey, desiredItem));
                continue;
            }

            var matchedIndex = currentIndex.Value;
            if (matchedCurrent[matchedIndex])
            {
                throw new DuplicateSyncMatchException(
                    typeof(TCurrent),
                    typeof(TDesired),
                    Convert.ToString(currentIdentities[matchedIndex].CanonicalKey) ?? string.Empty);
            }

            matchedCurrent[matchedIndex] = true;
            var currentItem = current[matchedIndex];
            var matchedKey = currentIdentities[matchedIndex].CanonicalKey;
            if (definition.AreEquivalent(currentItem, desiredItem))
            {
                unchanged.Add(new CrossSyncUnchanged<TCurrent, TDesired, TKey>(matchedKey, currentItem, desiredItem));
                continue;
            }

            updated.Add(new CrossSyncUpdate<TCurrent, TDesired, TKey, TDelta>(
                matchedKey,
                currentItem,
                desiredItem,
                definition.DeltaFactory(currentItem, desiredItem)));
        }

        for (var index = 0; index < current.Count; index++)
        {
            if (matchedCurrent[index])
            {
                continue;
            }

            var currentItem = current[index];
            var key = currentIdentities[index].CanonicalKey;
            if (definition.Mode == SyncMode.Upsert)
            {
                preserved.Add(new CrossSyncPreserved<TCurrent, TKey>(key, currentItem));
            }
            else
            {
                removed.Add(new CrossSyncRemoval<TCurrent, TKey>(key, currentItem));
            }
        }

        return new CrossSyncPlan<TCurrent, TDesired, TKey, TDelta>(
            added,
            removed,
            updated,
            unchanged,
            preserved);
    }

    private static void IndexIdentity<TKey>(
        Dictionary<TKey, List<int>> index,
        SyncIdentity<TKey> identity,
        int itemIndex,
        IEqualityComparer<TKey> comparer)
        where TKey : notnull
    {
        var keys = new HashSet<TKey>(comparer);
        AddIdentityKey(index, keys, identity.CanonicalKey, itemIndex);
        foreach (var fallbackKey in identity.FallbackKeys)
        {
            AddIdentityKey(index, keys, fallbackKey, itemIndex);
        }
    }

    private static void AddIdentityKey<TKey>(
        Dictionary<TKey, List<int>> index,
        HashSet<TKey> itemKeys,
        TKey key,
        int itemIndex)
        where TKey : notnull
    {
        if (!itemKeys.Add(key))
        {
            return;
        }

        if (!index.TryGetValue(key, out var matches))
        {
            matches = [];
            index.Add(key, matches);
        }

        matches.Add(itemIndex);
    }

    private static int? FindCurrentIndex<TKey>(
        IReadOnlyDictionary<TKey, List<int>> currentByIdentityKey,
        SyncIdentity<TKey> desiredIdentity,
        IEqualityComparer<TKey> comparer,
        Type currentType)
        where TKey : notnull
    {
        var attemptedKeys = new HashSet<TKey>(comparer);
        var canonicalMatch = FindCurrentIndexForKey(
            currentByIdentityKey,
            desiredIdentity.CanonicalKey,
            attemptedKeys,
            currentType);
        if (canonicalMatch is not null)
        {
            return canonicalMatch;
        }

        foreach (var fallbackKey in desiredIdentity.FallbackKeys)
        {
            var fallbackMatch = FindCurrentIndexForKey(
                currentByIdentityKey,
                fallbackKey,
                attemptedKeys,
                currentType);
            if (fallbackMatch is not null)
            {
                return fallbackMatch;
            }
        }

        return null;
    }

    private static int? FindCurrentIndexForKey<TKey>(
        IReadOnlyDictionary<TKey, List<int>> currentByIdentityKey,
        TKey key,
        HashSet<TKey> attemptedKeys,
        Type currentType)
        where TKey : notnull
    {
        if (!attemptedKeys.Add(key) || !currentByIdentityKey.TryGetValue(key, out var matches))
        {
            return null;
        }

        if (matches.Count > 1)
        {
            throw new AmbiguousSyncIdentityException(currentType, Convert.ToString(key) ?? string.Empty);
        }

        return matches[0];
    }

    private static Dictionary<TKey, TItem> Index<TItem, TKey>(
        IReadOnlyList<TItem> items,
        Func<TItem, TKey> keySelector,
        IEqualityComparer<TKey> comparer,
        string collectionName)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TItem>(items.Count, comparer);
        foreach (var item in items)
        {
            ArgumentNullException.ThrowIfNull(item);
            var key = keySelector(item);
            if (!result.TryAdd(key, item))
            {
                throw new DuplicateSyncKeyException(typeof(TItem), collectionName, Convert.ToString(key) ?? string.Empty);
            }
        }

        return result;
    }

}
