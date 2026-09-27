namespace Forge.Sync;

/// <summary>Immutable typed fact snapshot.</summary>
public sealed class FactSet
{
    private readonly IReadOnlyDictionary<long, IFactEntry> _entries;

    internal FactSet(IReadOnlyDictionary<long, IFactEntry> entries)
    {
        _entries = new Dictionary<long, IFactEntry>(entries);
    }

    /// <summary>Gets an empty fact set.</summary>
    public static FactSet Empty { get; } = new(new Dictionary<long, IFactEntry>());

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

    internal IReadOnlyDictionary<long, IFactEntry> Entries => _entries;
}
