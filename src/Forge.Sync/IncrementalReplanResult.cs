namespace Forge.Sync;

/// <summary>
/// Describes how an executable reconciliation plan changed after replanning.
/// </summary>
public sealed class IncrementalReplanResult<TOperationId>
{
    /// <summary>
    /// Creates an immutable incremental replanning result.
    /// </summary>
    public IncrementalReplanResult(
        IncrementalPlan<TOperationId> plan,
        IReadOnlyList<IncrementalPlanOperation<TOperationId>> retained,
        IReadOnlyList<IncrementalPlanOperation<TOperationId>> newlyRequired,
        IReadOnlyList<IncrementalPlanOperation<TOperationId>> noLongerRequired,
        IReadOnlyList<IncrementalPlanReplacement<TOperationId>> replaced)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(retained);
        ArgumentNullException.ThrowIfNull(newlyRequired);
        ArgumentNullException.ThrowIfNull(noLongerRequired);
        ArgumentNullException.ThrowIfNull(replaced);
        Plan = plan;
        Retained = Array.AsReadOnly(retained.ToArray());
        NewlyRequired = Array.AsReadOnly(newlyRequired.ToArray());
        NoLongerRequired = Array.AsReadOnly(noLongerRequired.ToArray());
        Replaced = Array.AsReadOnly(replaced.ToArray());
    }

    public IncrementalPlan<TOperationId> Plan { get; }
    public IReadOnlyList<IncrementalPlanOperation<TOperationId>> Retained { get; }
    public IReadOnlyList<IncrementalPlanOperation<TOperationId>> NewlyRequired { get; }
    public IReadOnlyList<IncrementalPlanOperation<TOperationId>> NoLongerRequired { get; }
    public IReadOnlyList<IncrementalPlanReplacement<TOperationId>> Replaced { get; }
    public bool HasPlanChanges => NewlyRequired.Count != 0 || NoLongerRequired.Count != 0 || Replaced.Count != 0;
}
