using Forge.Delta;

namespace Forge.Sync;

/// <summary>Defines edge identity, endpoints and equality semantics for topology reconciliation.</summary>
public sealed class TopologyEdgeDefinition<TEdge, TEdgeKey, TNodeKey, TDelta>
    where TEdgeKey : notnull
    where TNodeKey : notnull
    where TDelta : IDelta
{
    public TopologyEdgeDefinition(
        Func<TEdge, TEdgeKey> keySelector,
        Func<TEdge, TNodeKey> sourceKeySelector,
        Func<TEdge, TNodeKey> targetKeySelector,
        Func<TEdge, TEdge, bool> areEquivalent,
        Func<TEdge, TEdge, TDelta> deltaFactory,
        IEqualityComparer<TEdgeKey>? keyComparer = null)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(sourceKeySelector);
        ArgumentNullException.ThrowIfNull(targetKeySelector);
        ArgumentNullException.ThrowIfNull(areEquivalent);
        ArgumentNullException.ThrowIfNull(deltaFactory);
        KeySelector = keySelector;
        SourceKeySelector = sourceKeySelector;
        TargetKeySelector = targetKeySelector;
        AreEquivalent = areEquivalent;
        DeltaFactory = deltaFactory;
        KeyComparer = keyComparer ?? EqualityComparer<TEdgeKey>.Default;
    }

    public Func<TEdge, TEdgeKey> KeySelector { get; }
    public Func<TEdge, TNodeKey> SourceKeySelector { get; }
    public Func<TEdge, TNodeKey> TargetKeySelector { get; }
    public Func<TEdge, TEdge, bool> AreEquivalent { get; }
    public Func<TEdge, TEdge, TDelta> DeltaFactory { get; }
    public IEqualityComparer<TEdgeKey> KeyComparer { get; }
}
