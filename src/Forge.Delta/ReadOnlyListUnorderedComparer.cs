namespace Forge.Delta;

/// <summary>
/// Compares read-only lists without considering order while preserving duplicate multiplicity.
/// </summary>
/// <typeparam name="T">The list element type.</typeparam>
public sealed class ReadOnlyListUnorderedComparer<T> : IEqualityComparer<IReadOnlyList<T>>
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

        var counts = new Dictionary<ValueBox, int>();
        for (var index = 0; index < x.Count; index++)
        {
            var box = new ValueBox(x[index]);
            counts.TryGetValue(box, out var count);
            counts[box] = count + 1;
        }

        for (var index = 0; index < y.Count; index++)
        {
            var box = new ValueBox(y[index]);
            if (!counts.TryGetValue(box, out var count))
            {
                return false;
            }

            if (count == 1)
            {
                counts.Remove(box);
            }
            else
            {
                counts[box] = count - 1;
            }
        }

        return counts.Count == 0;
    }

    /// <inheritdoc />
    public int GetHashCode(IReadOnlyList<T> obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var comparer = EqualityComparer<T>.Default;
        var sum = 0;
        var xor = 0;
        for (var index = 0; index < obj.Count; index++)
        {
            var hash = obj[index] is null ? 0 : comparer.GetHashCode(obj[index]);
            sum = unchecked(sum + hash);
            xor ^= hash;
        }

        return HashCode.Combine(obj.Count, sum, xor);
    }

    private readonly struct ValueBox : IEquatable<ValueBox>
    {
        public ValueBox(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public bool Equals(ValueBox other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object? obj)
        {
            return obj is ValueBox other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value is null ? 0 : EqualityComparer<T>.Default.GetHashCode(Value);
        }
    }
}
