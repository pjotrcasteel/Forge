namespace Forge.Delta;

/// <summary>
/// Compares read-only dictionaries by key and value using the default equality comparers.
/// Enumeration order is ignored.
/// </summary>
/// <typeparam name="TKey">The dictionary key type.</typeparam>
/// <typeparam name="TValue">The dictionary value type.</typeparam>
public sealed class ReadOnlyDictionaryComparer<TKey, TValue> : IEqualityComparer<IReadOnlyDictionary<TKey, TValue>>
    where TKey : notnull
{
    /// <inheritdoc />
    public bool Equals(IReadOnlyDictionary<TKey, TValue>? x, IReadOnlyDictionary<TKey, TValue>? y)
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

        var valueComparer = EqualityComparer<TValue>.Default;
        foreach (var pair in left)
        {
            if (!right.TryGetValue(pair.Key, out var rightValue)
                || !valueComparer.Equals(pair.Value, rightValue))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public int GetHashCode(IReadOnlyDictionary<TKey, TValue> obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var keyComparer = EqualityComparer<TKey>.Default;
        var valueComparer = EqualityComparer<TValue>.Default;
        var sum = 0;
        var xor = 0;
        foreach (var pair in obj)
        {
            var pairHash = HashCode.Combine(
                keyComparer.GetHashCode(pair.Key),
                pair.Value is null ? 0 : valueComparer.GetHashCode(pair.Value));
            sum = unchecked(sum + pairHash);
            xor ^= pairHash;
        }

        return HashCode.Combine(obj.Count, sum, xor);
    }

    private static Dictionary<TKey, TValue>? Index(IReadOnlyDictionary<TKey, TValue> source)
    {
        var result = new Dictionary<TKey, TValue>(source.Count, EqualityComparer<TKey>.Default);
        foreach (var pair in source)
        {
            if (!result.TryAdd(pair.Key, pair.Value))
            {
                return null;
            }
        }

        return result;
    }
}
