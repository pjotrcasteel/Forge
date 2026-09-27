namespace Forge.Sync;

/// <summary>Defines strongly typed node identity for a provenance graph.</summary>
public sealed class ProvenanceDefinition<TNode, TKey>
    where TKey : notnull
{
    /// <summary>Creates a provenance definition.</summary>
    public ProvenanceDefinition(Func<TNode, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        KeySelector = keySelector;
        KeyComparer = comparer ?? EqualityComparer<TKey>.Default;
    }

    public Func<TNode, TKey> KeySelector { get; }
    public IEqualityComparer<TKey> KeyComparer { get; }
}
