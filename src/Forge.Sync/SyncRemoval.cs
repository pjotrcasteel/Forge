namespace Forge.Sync;

/// <summary>
/// Represents an item that exists only in the current collection.
/// </summary>
/// <typeparam name="T">The synchronized item type.</typeparam>
/// <typeparam name="TKey">The generated logical key type.</typeparam>
/// <param name="Key">The logical key that identifies the item.</param>
/// <param name="Current">The current item to remove.</param>
public sealed record SyncRemoval<T, TKey>(
    TKey Key,
    T Current);
