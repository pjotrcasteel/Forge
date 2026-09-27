namespace Forge.Sync;

/// <summary>Defines logical identity and semantic equivalence for execution-aware replanning.</summary>
public sealed class ExecutedReplanDefinition<TOperation, TKey>
    where TKey : notnull
{
    /// <summary>Creates a replanning definition.</summary>
    public ExecutedReplanDefinition(
        Func<TOperation, TKey> keySelector,
        Func<TOperation, TOperation, bool> areEquivalent,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(areEquivalent);
        KeySelector = keySelector;
        AreEquivalent = areEquivalent;
        KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
    }

    public Func<TOperation, TKey> KeySelector { get; }
    public Func<TOperation, TOperation, bool> AreEquivalent { get; }
    public IEqualityComparer<TKey> KeyComparer { get; }
}
