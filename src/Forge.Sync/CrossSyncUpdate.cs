using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Represents matching current and desired items whose comparable state differs.
/// </summary>
public sealed record CrossSyncUpdate<TCurrent, TDesired, TKey, TDelta>(
    TKey Key,
    TCurrent Current,
    TDesired Desired,
    TDelta Delta)
    where TDelta : IDelta;
