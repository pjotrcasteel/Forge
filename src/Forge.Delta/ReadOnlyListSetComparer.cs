namespace Forge.Delta;

/// <summary>
/// Compares read-only lists as sets: order and duplicate multiplicity are ignored.
/// </summary>
/// <typeparam name="T">The list element type.</typeparam>
public sealed class ReadOnlyListSetComparer<T> : IEqualityComparer<IReadOnlyList<T>>
{
    /// <inheritdoc />
    public bool Equals(IReadOnlyList<T>? x, IReadOnlyList<T>? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        var left = new HashSet<T>(x, EqualityComparer<T>.Default);
        var right = new HashSet<T>(y, EqualityComparer<T>.Default);
        return left.SetEquals(right);
    }

    /// <inheritdoc />
    public int GetHashCode(IReadOnlyList<T> obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var set = new HashSet<T>(obj, EqualityComparer<T>.Default);
        var xor = 0;
        foreach (var item in set)
        {
            xor ^= item is null ? 0 : EqualityComparer<T>.Default.GetHashCode(item);
        }

        return HashCode.Combine(set.Count, xor);
    }
}
