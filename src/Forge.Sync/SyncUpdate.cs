using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Represents an item that exists in both collections but whose state changed.
/// </summary>
/// <typeparam name="T">The synchronized item type.</typeparam>
/// <typeparam name="TKey">The generated logical key type.</typeparam>
/// <typeparam name="TDelta">The generated delta type.</typeparam>
/// <param name="Key">The logical key that identifies the item.</param>
/// <param name="Current">The current item.</param>
/// <param name="Desired">The desired item.</param>
/// <param name="Delta">The generated delta between current and desired state.</param>
public sealed record SyncUpdate<T, TKey, TDelta>(
    TKey Key,
    T Current,
    T Desired,
    TDelta Delta)
    where TDelta : IDelta;
