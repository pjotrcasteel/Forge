namespace Forge.Sync;

/// <summary>Groups replanning changes that are safe before execution starts.</summary>
public sealed class ExecutedReplanSafeChanges<TOperationId, TOperation, TKey>
    where TKey : notnull
{
    public ExecutedReplanSafeChanges(
        IReadOnlyList<RetainedPendingOperation<TOperationId, TOperation, TKey>> retained,
        IReadOnlyList<CancelledPendingOperation<TOperationId, TOperation, TKey>> cancelled,
        IReadOnlyList<ReplacedPendingOperation<TOperationId, TOperation, TKey>> replaced,
        IReadOnlyList<NewPlannedOperation<TOperationId, TOperation, TKey>> newlyPlanned)
    {
        Retained = Array.AsReadOnly(retained.ToArray());
        Cancelled = Array.AsReadOnly(cancelled.ToArray());
        Replaced = Array.AsReadOnly(replaced.ToArray());
        NewlyPlanned = Array.AsReadOnly(newlyPlanned.ToArray());
    }

    public IReadOnlyList<RetainedPendingOperation<TOperationId, TOperation, TKey>> Retained { get; }
    public IReadOnlyList<CancelledPendingOperation<TOperationId, TOperation, TKey>> Cancelled { get; }
    public IReadOnlyList<ReplacedPendingOperation<TOperationId, TOperation, TKey>> Replaced { get; }
    public IReadOnlyList<NewPlannedOperation<TOperationId, TOperation, TKey>> NewlyPlanned { get; }
}
