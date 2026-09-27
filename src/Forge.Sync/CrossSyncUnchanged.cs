namespace Forge.Sync;

/// <summary>
/// Represents matching current and desired items whose comparable state is equivalent.
/// </summary>
public sealed record CrossSyncUnchanged<TCurrent, TDesired, TKey>(
    TKey Key,
    TCurrent Current,
    TDesired Desired);
