namespace Forge.Sync;

/// <summary>
/// Represents a desired item that has no matching current item during cross-type reconciliation.
/// </summary>
public sealed record CrossSyncAddition<TDesired, TKey>(TKey Key, TDesired Desired);
