namespace Forge.Sync;

/// <summary>Replans desired operations while respecting work that is already running or completed.</summary>
public static class ExecutedReplanner
{
    /// <summary>Creates the initial tracked plan with application-owned operation identities.</summary>
    public static ExecutedPlan<TOperationId, TOperation, TKey> Create<TOperationId, TOperation, TKey>(
        IReadOnlyList<TOperation> operations,
        ExecutedReplanDefinition<TOperation, TKey> definition,
        Func<TOperation, TOperationId> operationIdFactory)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(operationIdFactory);
        var seen = new HashSet<TKey>(definition.KeyComparer);
        var tracked = new TrackedPlanOperation<TOperationId, TOperation, TKey>[operations.Count];
        for (var index = 0; index < operations.Count; index++)
        {
            var operation = operations[index];
            ArgumentNullException.ThrowIfNull(operation);
            var key = definition.KeySelector(operation);
            if (!seen.Add(key))
            {
                throw new DuplicateSyncKeyException(typeof(TOperation), nameof(operations), Convert.ToString(key) ?? string.Empty);
            }

            tracked[index] = new TrackedPlanOperation<TOperationId, TOperation, TKey>(operationIdFactory(operation), key, operation);
        }

        return new ExecutedPlan<TOperationId, TOperation, TKey>(tracked);
    }

    /// <summary>Replans against new desired operations using an immutable execution-state snapshot.</summary>
    public static ExecutedReplanResult<TOperationId, TOperation, TKey> Replan<TOperationId, TOperation, TKey, TState>(
        ExecutedPlan<TOperationId, TOperation, TKey> previous,
        IReadOnlyList<TOperation> nextOperations,
        ExecutionSnapshot<TOperationId, TState> execution,
        IExecutionStateClassifier<TState> stateClassifier,
        ExecutedReplanDefinition<TOperation, TKey> definition,
        Func<TOperation, TOperationId> operationIdFactory)
        where TOperationId : notnull
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(nextOperations);
        ArgumentNullException.ThrowIfNull(execution);
        ArgumentNullException.ThrowIfNull(stateClassifier);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(operationIdFactory);

        var desiredByKey = Index(nextOperations, definition);
        var nextByKey = new Dictionary<TKey, TrackedPlanOperation<TOperationId, TOperation, TKey>>(definition.KeyComparer);
        var retained = new List<RetainedPendingOperation<TOperationId, TOperation, TKey>>();
        var cancelled = new List<CancelledPendingOperation<TOperationId, TOperation, TKey>>();
        var replaced = new List<ReplacedPendingOperation<TOperationId, TOperation, TKey>>();
        var newlyPlanned = new List<NewPlannedOperation<TOperationId, TOperation, TKey>>();
        var completed = new List<LockedCompletedOperation<TOperationId, TOperation, TKey>>();
        var running = new List<PreservedRunningOperation<TOperationId, TOperation, TKey>>();
        var compensationRemovals = new List<CompensationForRemoval<TOperationId, TOperation, TKey>>();
        var compensationReplacements = new List<CompensationForReplacement<TOperationId, TOperation, TKey>>();
        var runningRemovals = new List<RunningRemovalConflict<TOperationId, TOperation, TKey>>();
        var runningReplacements = new List<RunningReplacementConflict<TOperationId, TOperation, TKey>>();

        foreach (var old in previous.Operations)
        {
            if (!execution.TryGetState(old.OperationId, out var state))
            {
                throw new KeyNotFoundException("Execution state is required for every previously tracked operation.");
            }

            var disposition = stateClassifier.Classify(state);
            if (!desiredByKey.TryGetValue(old.Key, out var desired))
            {
                ClassifyRemoval(old, disposition, cancelled, compensationRemovals, runningRemovals);
                continue;
            }

            desiredByKey.Remove(old.Key);
            if (definition.AreEquivalent(old.Operation, desired))
            {
                var current = new TrackedPlanOperation<TOperationId, TOperation, TKey>(old.OperationId, old.Key, desired);
                nextByKey.Add(old.Key, current);
                ClassifyRetained(current, disposition, retained, completed, running);
                continue;
            }

            var replacement = new TrackedPlanOperation<TOperationId, TOperation, TKey>(
                operationIdFactory(desired),
                old.Key,
                desired);
            nextByKey.Add(old.Key, replacement);
            ClassifyReplacement(
                old,
                replacement,
                disposition,
                replaced,
                compensationReplacements,
                runningReplacements);
        }

        foreach (var operation in nextOperations)
        {
            var key = definition.KeySelector(operation);
            if (!desiredByKey.ContainsKey(key))
            {
                continue;
            }

            var created = new TrackedPlanOperation<TOperationId, TOperation, TKey>(operationIdFactory(operation), key, operation);
            nextByKey.Add(key, created);
            desiredByKey.Remove(key);
            newlyPlanned.Add(new NewPlannedOperation<TOperationId, TOperation, TKey>(created));
        }

        var orderedNext = nextOperations.Select(operation => nextByKey[definition.KeySelector(operation)]).ToArray();
        var safe = new ExecutedReplanSafeChanges<TOperationId, TOperation, TKey>(retained, cancelled, replaced, newlyPlanned);
        var locked = new ExecutedReplanLockedWork<TOperationId, TOperation, TKey>(completed, running);
        var interventions = new ExecutedReplanInterventions<TOperationId, TOperation, TKey>(
            compensationRemovals,
            compensationReplacements,
            runningRemovals,
            runningReplacements);
        return new ExecutedReplanResult<TOperationId, TOperation, TKey>(
            new ExecutedPlan<TOperationId, TOperation, TKey>(orderedNext),
            safe,
            locked,
            interventions);
    }

    private static Dictionary<TKey, TOperation> Index<TOperation, TKey>(
        IReadOnlyList<TOperation> operations,
        ExecutedReplanDefinition<TOperation, TKey> definition)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TOperation>(operations.Count, definition.KeyComparer);
        foreach (var operation in operations)
        {
            ArgumentNullException.ThrowIfNull(operation);
            var key = definition.KeySelector(operation);
            if (!result.TryAdd(key, operation))
            {
                throw new DuplicateSyncKeyException(typeof(TOperation), nameof(operations), Convert.ToString(key) ?? string.Empty);
            }
        }

        return result;
    }

    private static void ClassifyRemoval<TOperationId, TOperation, TKey>(
        TrackedPlanOperation<TOperationId, TOperation, TKey> previous,
        ExecutionDisposition disposition,
        List<CancelledPendingOperation<TOperationId, TOperation, TKey>> cancelled,
        List<CompensationForRemoval<TOperationId, TOperation, TKey>> compensation,
        List<RunningRemovalConflict<TOperationId, TOperation, TKey>> running)
        where TKey : notnull
    {
        switch (disposition)
        {
            case ExecutionDisposition.NotStarted:
                cancelled.Add(new CancelledPendingOperation<TOperationId, TOperation, TKey>(previous));
                break;
            case ExecutionDisposition.Completed:
                compensation.Add(new CompensationForRemoval<TOperationId, TOperation, TKey>(previous));
                break;
            case ExecutionDisposition.Running:
                running.Add(new RunningRemovalConflict<TOperationId, TOperation, TKey>(previous));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(disposition));
        }
    }

    private static void ClassifyRetained<TOperationId, TOperation, TKey>(
        TrackedPlanOperation<TOperationId, TOperation, TKey> current,
        ExecutionDisposition disposition,
        List<RetainedPendingOperation<TOperationId, TOperation, TKey>> retained,
        List<LockedCompletedOperation<TOperationId, TOperation, TKey>> completed,
        List<PreservedRunningOperation<TOperationId, TOperation, TKey>> running)
        where TKey : notnull
    {
        switch (disposition)
        {
            case ExecutionDisposition.NotStarted:
                retained.Add(new RetainedPendingOperation<TOperationId, TOperation, TKey>(current));
                break;
            case ExecutionDisposition.Completed:
                completed.Add(new LockedCompletedOperation<TOperationId, TOperation, TKey>(current));
                break;
            case ExecutionDisposition.Running:
                running.Add(new PreservedRunningOperation<TOperationId, TOperation, TKey>(current));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(disposition));
        }
    }

    private static void ClassifyReplacement<TOperationId, TOperation, TKey>(
        TrackedPlanOperation<TOperationId, TOperation, TKey> previous,
        TrackedPlanOperation<TOperationId, TOperation, TKey> desired,
        ExecutionDisposition disposition,
        List<ReplacedPendingOperation<TOperationId, TOperation, TKey>> replaced,
        List<CompensationForReplacement<TOperationId, TOperation, TKey>> compensation,
        List<RunningReplacementConflict<TOperationId, TOperation, TKey>> running)
        where TKey : notnull
    {
        switch (disposition)
        {
            case ExecutionDisposition.NotStarted:
                replaced.Add(new ReplacedPendingOperation<TOperationId, TOperation, TKey>(previous, desired));
                break;
            case ExecutionDisposition.Completed:
                compensation.Add(new CompensationForReplacement<TOperationId, TOperation, TKey>(previous, desired));
                break;
            case ExecutionDisposition.Running:
                running.Add(new RunningReplacementConflict<TOperationId, TOperation, TKey>(previous, desired));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(disposition));
        }
    }
}
