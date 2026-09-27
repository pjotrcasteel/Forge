using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Defines logical identity, semantic equality and Delta creation for cross-type reconciliation.
/// </summary>
public sealed class CrossSyncDefinition<TCurrent, TDesired, TKey, TDelta>
    where TKey : notnull
    where TDelta : IDelta
{
    public CrossSyncDefinition(
        Func<TCurrent, TKey> currentKeySelector,
        Func<TDesired, TKey> desiredKeySelector,
        Func<TCurrent, TDesired, bool> areEquivalent,
        Func<TCurrent, TDesired, TDelta> deltaFactory,
        SyncMode mode = SyncMode.Replace,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        ArgumentNullException.ThrowIfNull(currentKeySelector);
        ArgumentNullException.ThrowIfNull(desiredKeySelector);
        ArgumentNullException.ThrowIfNull(areEquivalent);
        ArgumentNullException.ThrowIfNull(deltaFactory);
        if (mode is not SyncMode.Replace and not SyncMode.Upsert)
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }

        CurrentKeySelector = currentKeySelector;
        DesiredKeySelector = desiredKeySelector;
        AreEquivalent = areEquivalent;
        DeltaFactory = deltaFactory;
        Mode = mode;
        KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
    }

    public Func<TCurrent, TKey> CurrentKeySelector { get; }
    public Func<TDesired, TKey> DesiredKeySelector { get; }
    public Func<TCurrent, TDesired, bool> AreEquivalent { get; }
    public Func<TCurrent, TDesired, TDelta> DeltaFactory { get; }
    public SyncMode Mode { get; }
    public IEqualityComparer<TKey> KeyComparer { get; }
}
