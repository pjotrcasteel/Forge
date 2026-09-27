namespace Forge.Sync;

/// <summary>
/// Represents a current-only item intentionally preserved by an upsert reconciliation.
/// </summary>
public sealed record CrossSyncPreserved<TCurrent, TKey>(TKey Key, TCurrent Current);
