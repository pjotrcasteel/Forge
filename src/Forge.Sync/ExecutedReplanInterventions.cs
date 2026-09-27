namespace Forge.Sync;

/// <summary>Groups replanning outcomes that require application policy before the new plan can proceed safely.</summary>
public sealed class ExecutedReplanInterventions<TOperationId, TOperation, TKey>
    where TKey : notnull
{
    public ExecutedReplanInterventions(
        IReadOnlyList<CompensationForRemoval<TOperationId, TOperation, TKey>> compensationRemovals,
        IReadOnlyList<CompensationForReplacement<TOperationId, TOperation, TKey>> compensationReplacements,
        IReadOnlyList<RunningRemovalConflict<TOperationId, TOperation, TKey>> runningRemovals,
        IReadOnlyList<RunningReplacementConflict<TOperationId, TOperation, TKey>> runningReplacements)
    {
        CompensationRemovals = Array.AsReadOnly(compensationRemovals.ToArray());
        CompensationReplacements = Array.AsReadOnly(compensationReplacements.ToArray());
        RunningRemovals = Array.AsReadOnly(runningRemovals.ToArray());
        RunningReplacements = Array.AsReadOnly(runningReplacements.ToArray());
    }

    public IReadOnlyList<CompensationForRemoval<TOperationId, TOperation, TKey>> CompensationRemovals { get; }
    public IReadOnlyList<CompensationForReplacement<TOperationId, TOperation, TKey>> CompensationReplacements { get; }
    public IReadOnlyList<RunningRemovalConflict<TOperationId, TOperation, TKey>> RunningRemovals { get; }
    public IReadOnlyList<RunningReplacementConflict<TOperationId, TOperation, TKey>> RunningReplacements { get; }
    public bool HasAny => CompensationRemovals.Count != 0 || CompensationReplacements.Count != 0
        || RunningRemovals.Count != 0 || RunningReplacements.Count != 0;
}
