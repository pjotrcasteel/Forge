namespace Forge.Sync;

/// <summary>
/// Explains why one logical item has a particular reconciliation outcome.
/// </summary>
public sealed record PlanExplanationEntry<TKey>(
    TKey Key,
    PlanExplanationAction Action,
    IReadOnlyList<PlanExplanationNode> Reasons);
