namespace Forge.Delta;

/// <summary>
/// Compares two read-only lists by count and element order using <see cref="EqualityComparer{T}.Default"/>.
/// </summary>
/// <typeparam name="T">The list element type.</typeparam>
public sealed class ReadOnlyListSequenceComparer<T> : IEqualityComparer<IReadOnlyList<T>>
{
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

        var comparer = EqualityComparer<T>.Default;
        for (var index = 0; index < x.Count; index++)
        {
            if (!comparer.Equals(x[index], y[index]))
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

        var hashCode = new HashCode();
        var comparer = EqualityComparer<T>.Default;
        for (var index = 0; index < obj.Count; index++)
        {
            hashCode.Add(obj[index], comparer);
        }

        return hashCode.ToHashCode();
    }
}
