namespace Forge.Sync;

/// <summary>Common contract for a single streaming reconciliation result.</summary>
public interface IStreamingSyncStep<out TKey>
{
    TKey Key { get; }
    StreamingSyncChangeKind Kind { get; }
}
