using Forge.Delta;

namespace Forge.Sync;

/// <summary>Associates an update and its Delta with the application operation selected for it.</summary>
public sealed record PlannedUpdate<T, TKey, TDelta, TOperation>(
    TKey Key,
    T Current,
    T Desired,
    TDelta Delta,
    TOperation Operation)
    where TDelta : IDelta;
