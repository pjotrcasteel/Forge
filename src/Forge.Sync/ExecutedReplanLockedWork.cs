namespace Forge.Sync;

/// <summary>Groups already-started work whose semantics remain valid.</summary>
public sealed class ExecutedReplanLockedWork<TOperationId, TOperation, TKey>
    where TKey : notnull
{
    public ExecutedReplanLockedWork(
        IReadOnlyList<LockedCompletedOperation<TOperationId, TOperation, TKey>> completed,
        IReadOnlyList<PreservedRunningOperation<TOperationId, TOperation, TKey>> running)
    {
        Completed = Array.AsReadOnly(completed.ToArray());
        Running = Array.AsReadOnly(running.ToArray());
    }

    public IReadOnlyList<LockedCompletedOperation<TOperationId, TOperation, TKey>> Completed { get; }
    public IReadOnlyList<PreservedRunningOperation<TOperationId, TOperation, TKey>> Running { get; }
}
