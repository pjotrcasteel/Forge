using Forge.Delta;

namespace Forge.Sync;

/// <summary>Defines typed identity and semantic comparison for one category of plan element.</summary>
public sealed class PlanElementDefinition<TItem, TKey, TDelta>
    where TKey : notnull
    where TDelta : IDelta
{
    /// <summary>Creates a plan element definition.</summary>
    public PlanElementDefinition(
        Func<TItem, TKey> keySelector,
        Func<TItem, TItem, bool> areEquivalent,
        Func<TItem, TItem, TDelta> deltaFactory,
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

    public Func<TItem, TKey> KeySelector { get; }
    public Func<TItem, TItem, bool> AreEquivalent { get; }
    public Func<TItem, TItem, TDelta> DeltaFactory { get; }
    public IEqualityComparer<TKey> KeyComparer { get; }
}
