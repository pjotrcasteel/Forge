namespace Forge.Delta;

/// <summary>
/// Compares read-only lists by logical key and item value without considering order.
/// </summary>
/// <typeparam name="T">The list item type.</typeparam>
/// <typeparam name="TKey">The non-null logical key type.</typeparam>
/// <typeparam name="TKeySelector">The constructible key selector type.</typeparam>
public sealed class ReadOnlyListKeyedComparer<T, TKey, TKeySelector> : IEqualityComparer<IReadOnlyList<T>>
    where TKey : notnull
    where TKeySelector : IKeySelector<T, TKey>, new()
{
    private static readonly TKeySelector KeySelector = new();

    /// <inheritdoc />
    public bool Equals(IReadOnlyList<T>? x, IReadOnlyList<T>? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null || x.Count != y.Count)
        {
            return false;
        }

        var left = Index(x);
        var right = Index(y);
        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        var itemComparer = EqualityComparer<T>.Default;
        foreach (var pair in left)
        {
            if (!right.TryGetValue(pair.Key, out var rightItem)
                || !itemComparer.Equals(pair.Value, rightItem))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public int GetHashCode(IReadOnlyList<T> obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var keyComparer = EqualityComparer<TKey>.Default;
        var itemComparer = EqualityComparer<T>.Default;
        var sum = 0;
        var xor = 0;
        for (var index = 0; index < obj.Count; index++)
        {
            var item = obj[index];
            var key = KeySelector.GetKey(item);
            var pairHash = HashCode.Combine(
                key is null ? 0 : keyComparer.GetHashCode(key),
                item is null ? 0 : itemComparer.GetHashCode(item));
            sum = unchecked(sum + pairHash);
            xor ^= pairHash;
        }

        return HashCode.Combine(obj.Count, sum, xor);
    }

    private static Dictionary<TKey, T>? Index(IReadOnlyList<T> items)
    {
        var result = new Dictionary<TKey, T>(items.Count, EqualityComparer<TKey>.Default);
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var key = KeySelector.GetKey(item);
            if (key is null || !result.TryAdd(key, item))
            {
                return null;
            }
        }

        return result;
    }
}
