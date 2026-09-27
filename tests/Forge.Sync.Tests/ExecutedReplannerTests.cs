namespace Forge.Sync.Tests;

[TestClass]
public sealed class ExecutedReplannerTests
{
    [TestMethod]
    public void Replan_ShouldRespectCompletedRunningAndPendingWorkWithoutNullOutcomeFields()
    {
        Operation[] initial =
        [
            new(new OperationKey(1), 10),
            new(new OperationKey(2), 20),
            new(new OperationKey(3), 30),
            new(new OperationKey(4), 40),
            new(new OperationKey(5), 50)
        ];
        var next = new[]
        {
            new Operation(new OperationKey(1), 10),
            new Operation(new OperationKey(2), 25),
            new Operation(new OperationKey(3), 35),
            new Operation(new OperationKey(6), 60)
        };
        var nextId = 100;
        var definition = Definition();
        var tracked = ExecutedReplanner.Create(initial, definition, _ => new OperationId(nextId++));
        var states = new Dictionary<OperationId, ExecutionState>
        {
            [tracked.Operations[0].OperationId] = ExecutionState.Completed,
            [tracked.Operations[1].OperationId] = ExecutionState.Completed,
            [tracked.Operations[2].OperationId] = ExecutionState.Running,
            [tracked.Operations[3].OperationId] = ExecutionState.Waiting,
            [tracked.Operations[4].OperationId] = ExecutionState.Running
        };

        var result = ExecutedReplanner.Replan(
            tracked,
            next,
            new ExecutionSnapshot<OperationId, ExecutionState>(states),
            new StateClassifier(),
            definition,
            _ => new OperationId(nextId++));

        Assert.AreEqual(1, result.LockedWork.Completed.Count);
        Assert.AreEqual(1, result.Interventions.CompensationReplacements.Count);
        Assert.AreEqual(1, result.Interventions.RunningReplacements.Count);
        Assert.AreEqual(1, result.SafeChanges.Cancelled.Count);
        Assert.AreEqual(1, result.Interventions.RunningRemovals.Count);
        Assert.AreEqual(1, result.SafeChanges.NewlyPlanned.Count);
        Assert.IsFalse(result.CanProceedWithoutIntervention);
        Assert.AreEqual(4, result.NextPlan.Operations.Count);
    }

    [TestMethod]
    public void Replan_WhenPendingOperationIsEquivalent_ShouldRetainStableOperationIdentity()
    {
        Operation[] initial = [new(new OperationKey(1), 10)];
        var definition = Definition();
        var tracked = ExecutedReplanner.Create(initial, definition, _ => new OperationId(42));
        var snapshot = new ExecutionSnapshot<OperationId, ExecutionState>(
            new Dictionary<OperationId, ExecutionState> { [new OperationId(42)] = ExecutionState.Waiting });

        var result = ExecutedReplanner.Replan(
            tracked,
            [new Operation(new OperationKey(1), 10)],
            snapshot,
            new StateClassifier(),
            definition,
            _ => new OperationId(99));

        Assert.AreEqual(new OperationId(42), result.NextPlan.Operations.Single().OperationId);
        Assert.AreEqual(1, result.SafeChanges.Retained.Count);
        Assert.IsTrue(result.CanProceedWithoutIntervention);
    }

    private static ExecutedReplanDefinition<Operation, OperationKey> Definition()
        => new(static operation => operation.Key, static (left, right) => left.Value == right.Value);

    private sealed record Operation(OperationKey Key, int Value);
    private readonly record struct OperationKey(int Value);
    private readonly record struct OperationId(int Value);

    private enum ExecutionState
    {
        Waiting,
        Running,
        Completed
    }

    private sealed class StateClassifier : IExecutionStateClassifier<ExecutionState>
    {
        public ExecutionDisposition Classify(ExecutionState state)
            => state switch
            {
                ExecutionState.Waiting => ExecutionDisposition.NotStarted,
                ExecutionState.Running => ExecutionDisposition.Running,
                ExecutionState.Completed => ExecutionDisposition.Completed,
                _ => throw new ArgumentOutOfRangeException(nameof(state))
            };
    }
}
