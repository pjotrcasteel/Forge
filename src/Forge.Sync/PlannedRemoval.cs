namespace Forge.Sync;

/// <summary>Associates a removed current item with the application operation selected for it.</summary>
public sealed record PlannedRemoval<T, TKey, TOperation>(
    TKey Key,
    T Current,
    TOperation Operation);
