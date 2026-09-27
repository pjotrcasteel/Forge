namespace Forge.Sync;

/// <summary>Associates an added desired item with the application operation selected for it.</summary>
public sealed record PlannedAddition<T, TKey, TOperation>(
    TKey Key,
    T Desired,
    TOperation Operation);
