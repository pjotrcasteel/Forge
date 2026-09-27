namespace Forge.Sync;

/// <summary>
/// Represents a current item intentionally preserved because it was absent from a partial upsert payload.
/// </summary>
/// <typeparam name="T">The synchronized item type.</typeparam>
/// <typeparam name="TKey">The generated logical key type.</typeparam>
/// <param name="Key">The logical key that identifies the item.</param>
/// <param name="Current">The current item that remains untouched.</param>
public sealed record SyncPreserved<T, TKey>(
    TKey Key,
    T Current);
