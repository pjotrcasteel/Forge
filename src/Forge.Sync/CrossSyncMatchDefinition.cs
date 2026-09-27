using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Defines ordered multi-key identity, semantic equality and Delta creation for cross-type reconciliation.
/// </summary>
public sealed class CrossSyncMatchDefinition<TCurrent, TDesired, TKey, TDelta>
    where TKey : notnull
    where TDelta : IDelta
{
    /// <summary>
    /// Initializes a cross-type reconciliation definition that can match by a canonical key and ordered fallback keys.
    /// </summary>
    public CrossSyncMatchDefinition(
        Func<TCurrent, SyncIdentity<TKey>> currentIdentitySelector,
        Func<TDesired, SyncIdentity<TKey>> desiredIdentitySelector,
        Func<TCurrent, TDesired, bool> areEquivalent,
        Func<TCurrent, TDesired, TDelta> deltaFactory,
        SyncMode mode = SyncMode.Replace,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        ArgumentNullException.ThrowIfNull(currentIdentitySelector);
        ArgumentNullException.ThrowIfNull(desiredIdentitySelector);
        ArgumentNullException.ThrowIfNull(areEquivalent);
        ArgumentNullException.ThrowIfNull(deltaFactory);
        if (mode is not SyncMode.Replace and not SyncMode.Upsert)
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }

        CurrentIdentitySelector = currentIdentitySelector;
        DesiredIdentitySelector = desiredIdentitySelector;
        AreEquivalent = areEquivalent;
        DeltaFactory = deltaFactory;
        Mode = mode;
        KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
    }

    /// <summary>Gets the identity selector for current items.</summary>
    public Func<TCurrent, SyncIdentity<TKey>> CurrentIdentitySelector { get; }

    /// <summary>Gets the identity selector for desired items.</summary>
    public Func<TDesired, SyncIdentity<TKey>> DesiredIdentitySelector { get; }

    /// <summary>Gets the semantic equality comparison.</summary>
    public Func<TCurrent, TDesired, bool> AreEquivalent { get; }

    /// <summary>Gets the Delta factory used for matched items whose state differs.</summary>
    public Func<TCurrent, TDesired, TDelta> DeltaFactory { get; }

    /// <summary>Gets the reconciliation mode.</summary>
    public SyncMode Mode { get; }

    /// <summary>Gets the comparer used for canonical and fallback keys.</summary>
    public IEqualityComparer<TKey> KeyComparer { get; }
}
