namespace Forge.Sync;

/// <summary>
/// Describes a canonical synchronization key together with ordered fallback keys that may identify the same logical item.
/// </summary>
/// <typeparam name="TKey">The key type.</typeparam>
public sealed class SyncIdentity<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Initializes a synchronization identity.
    /// </summary>
    /// <param name="canonicalKey">The key exposed by reconciliation results for this item.</param>
    /// <param name="fallbackKeys">Ordered fallback keys used only when an earlier key does not match.</param>
    public SyncIdentity(TKey canonicalKey, IReadOnlyList<TKey>? fallbackKeys = null)
    {
        CanonicalKey = canonicalKey;
        FallbackKeys = fallbackKeys is null
            ? Array.Empty<TKey>()
            : Array.AsReadOnly(fallbackKeys.ToArray());
    }

    /// <summary>
    /// Gets the canonical key exposed by reconciliation results.
    /// </summary>
    public TKey CanonicalKey { get; }

    /// <summary>
    /// Gets ordered fallback keys. Earlier entries have higher matching priority.
    /// </summary>
    public IReadOnlyList<TKey> FallbackKeys { get; }
}
