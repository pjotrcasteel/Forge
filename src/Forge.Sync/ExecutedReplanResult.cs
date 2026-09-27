namespace Forge.Sync;

/// <summary>Complete strongly typed result of replanning around already executed or running work.</summary>
public sealed class ExecutedReplanResult<TOperationId, TOperation, TKey>
    where TKey : notnull
{
    public ExecutedReplanResult(
        ExecutedPlan<TOperationId, TOperation, TKey> nextPlan,
        ExecutedReplanSafeChanges<TOperationId, TOperation, TKey> safeChanges,
        ExecutedReplanLockedWork<TOperationId, TOperation, TKey> lockedWork,
        ExecutedReplanInterventions<TOperationId, TOperation, TKey> interventions)
    {
        NextPlan = nextPlan;
        SafeChanges = safeChanges;
        LockedWork = lockedWork;
        Interventions = interventions;
    }

    public ExecutedPlan<TOperationId, TOperation, TKey> NextPlan { get; }
    public ExecutedReplanSafeChanges<TOperationId, TOperation, TKey> SafeChanges { get; }
    public ExecutedReplanLockedWork<TOperationId, TOperation, TKey> LockedWork { get; }
    public ExecutedReplanInterventions<TOperationId, TOperation, TKey> Interventions { get; }
    public bool CanProceedWithoutIntervention => !Interventions.HasAny;
}
