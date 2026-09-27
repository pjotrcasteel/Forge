namespace Forge.Sync;

/// <summary>Monotonic typed fact builder used during derivation.</summary>
public sealed class FactSetBuilder
{
    private readonly Dictionary<long, IFactEntry> _entries;

    public FactSetBuilder(FactSet initial)
    {
        ArgumentNullException.ThrowIfNull(initial);
        _entries = new Dictionary<long, IFactEntry>(initial.Entries);
    }

    public int Count => _entries.Count;

    public bool Contains<T>(FactKey<T> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _entries.ContainsKey(key.Id);
    }

    public bool TryGet<T>(FactKey<T> key, out T value)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (_entries.TryGetValue(key.Id, out var entry) && entry is FactEntry<T> typed)
        {
            value = typed.Value;
            return true;
        }

        value = default!;
        return false;
    }

    public T Get<T>(FactKey<T> key)
        => TryGet(key, out var value)
            ? value
            : throw new KeyNotFoundException("The requested typed fact is not present.");

    public bool TryAdd<T>(FactKey<T> key, T value, IEqualityComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (!_entries.TryGetValue(key.Id, out var existing))
        {
            _entries.Add(key.Id, new FactEntry<T>(value));
            return true;
        }

        if (existing is FactEntry<T> typed && (comparer ?? EqualityComparer<T>.Default).Equals(typed.Value, value))
        {
            return false;
        }

        throw new FactConflictException(typeof(T));
    }

    public FactSet Build() => new(_entries);
}
