namespace Forge.Sync;

/// <summary>
/// Represents an item that exists in both collections with equivalent state.
/// </summary>
/// <typeparam name="T">The synchronized item type.</typeparam>
/// <typeparam name="TKey">The generated logical key type.</typeparam>
/// <param name="Key">The logical key that identifies the item.</param>
/// <param name="Current">The current item.</param>
/// <param name="Desired">The equivalent desired item.</param>
public sealed record SyncUnchanged<T, TKey>(
    TKey Key,
    T Current,
    T Desired);
