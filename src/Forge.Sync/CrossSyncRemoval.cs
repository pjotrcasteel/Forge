namespace Forge.Sync;

/// <summary>
/// Represents a current item that has no matching desired item during cross-type reconciliation.
/// </summary>
public sealed record CrossSyncRemoval<TCurrent, TKey>(TKey Key, TCurrent Current);
