using Forge.Delta;

namespace Forge.Sync;

/// <summary>Defines reflection-free node identity and equality semantics for topology reconciliation.</summary>
public sealed class TopologyNodeDefinition<TNode, TKey, TDelta>
    where TKey : notnull
    where TDelta : IDelta
{
    public TopologyNodeDefinition(
        Func<TNode, TKey> keySelector,
        Func<TNode, TNode, bool> areEquivalent,
        Func<TNode, TNode, TDelta> deltaFactory,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(areEquivalent);
        ArgumentNullException.ThrowIfNull(deltaFactory);
        KeySelector = keySelector;
        AreEquivalent = areEquivalent;
        DeltaFactory = deltaFactory;
        KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
    }

    public Func<TNode, TKey> KeySelector { get; }
    public Func<TNode, TNode, bool> AreEquivalent { get; }
    public Func<TNode, TNode, TDelta> DeltaFactory { get; }
    public IEqualityComparer<TKey> KeyComparer { get; }
}
