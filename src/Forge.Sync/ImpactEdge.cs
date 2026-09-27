namespace Forge.Sync;

/// <summary>
/// Declares that an impact on <paramref name="Source"/> may propagate to <paramref name="Target"/> for the supplied reason.
/// </summary>
public sealed record ImpactEdge<TKey, TReason>(
    TKey Source,
    TKey Target,
    TReason Reason);
