using Forge.Delta;

namespace Forge.Sync;

public sealed record CrossPlannedAddition<TDesired, TKey, TOperation>(
    TKey Key,
    TDesired Desired,
    TOperation Operation);

public sealed record CrossPlannedRemoval<TCurrent, TKey, TOperation>(
    TKey Key,
    TCurrent Current,
    TOperation Operation);

public sealed record CrossPlannedUpdate<TCurrent, TDesired, TKey, TDelta, TOperation>(
    TKey Key,
    TCurrent Current,
    TDesired Desired,
    TDelta Delta,
    TOperation Operation)
    where TDelta : IDelta;
