using Forge.Delta;

namespace Forge.Sync;

public sealed record StreamingSyncAddition<TDesired, TKey>(TKey Key, TDesired Desired) : IStreamingSyncStep<TKey>
{
    public StreamingSyncChangeKind Kind => StreamingSyncChangeKind.Added;
}

public sealed record StreamingSyncRemoval<TCurrent, TKey>(TKey Key, TCurrent Current) : IStreamingSyncStep<TKey>
{
    public StreamingSyncChangeKind Kind => StreamingSyncChangeKind.Removed;
}

public sealed record StreamingSyncPreserved<TCurrent, TKey>(TKey Key, TCurrent Current) : IStreamingSyncStep<TKey>
{
    public StreamingSyncChangeKind Kind => StreamingSyncChangeKind.Preserved;
}

public sealed record StreamingSyncUnchanged<TCurrent, TDesired, TKey>(
    TKey Key,
    TCurrent Current,
    TDesired Desired) : IStreamingSyncStep<TKey>
{
    public StreamingSyncChangeKind Kind => StreamingSyncChangeKind.Unchanged;
}

public sealed record StreamingSyncUpdate<TCurrent, TDesired, TKey, TDelta>(
    TKey Key,
    TCurrent Current,
    TDesired Desired,
    TDelta Delta) : IStreamingSyncStep<TKey>
    where TDelta : IDelta
{
    public StreamingSyncChangeKind Kind => StreamingSyncChangeKind.Updated;
}
