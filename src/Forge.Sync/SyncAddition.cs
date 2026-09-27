namespace Forge.Sync;

/// <summary>
/// Represents an item that exists only in the desired collection.
/// </summary>
/// <typeparam name="T">The synchronized item type.</typeparam>
/// <typeparam name="TKey">The generated logical key type.</typeparam>
/// <param name="Key">The logical key that identifies the item.</param>
/// <param name="Desired">The desired item to add.</param>
public sealed record SyncAddition<T, TKey>(
    TKey Key,
    T Desired);
