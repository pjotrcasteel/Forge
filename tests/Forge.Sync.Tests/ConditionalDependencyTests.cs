namespace Forge.Sync.Tests;

[TestClass]
public sealed class ConditionalDependencyTests
{
    [TestMethod]
    public void Evaluate_WhenRequiredStateIsObserved_ShouldSatisfyDependency()
    {
        var dependency = new ConditionalDependency<OperationId, OperationState>(
            new OperationId(1),
            new OperationId(2),
            new StateEqualsCondition<OperationState>(OperationState.Completed));
        var snapshot = Snapshot((1, OperationState.Completed));

        var result = ConditionalDependencyEvaluator.Evaluate([dependency], snapshot);

        Assert.IsTrue(result.AllSatisfied);
        Assert.AreEqual(1, result.Satisfied.Count);
        Assert.AreEqual(0, result.Blocked.Count);
    }

    [TestMethod]
    public void Evaluate_WhenCustomTypedConditionRejectsState_ShouldExposeObservedState()
    {
        var dependency = new ConditionalDependency<OperationId, OperationState>(
            new OperationId(1),
            new OperationId(2),
            new AtLeastCondition(OperationState.InProgress));
        var snapshot = Snapshot((1, OperationState.Waiting));

        var result = ConditionalDependencyEvaluator.Evaluate([dependency], snapshot);

        Assert.IsFalse(result.AllSatisfied);
        Assert.AreEqual(ConditionalDependencyBlockReason.StateConditionNotSatisfied, result.Blocked[0].Reason);
        Assert.IsTrue(result.Blocked[0].ObservedState.HasValue);
        Assert.AreEqual(OperationState.Waiting, result.Blocked[0].ObservedState.Value);
    }

    [TestMethod]
    public void Evaluate_WhenPredecessorStateIsMissing_ShouldNotUseDefaultStateAsSentinel()
    {
        var dependency = new ConditionalDependency<OperationId, OperationState>(
            new OperationId(1),
            new OperationId(2),
            new StateEqualsCondition<OperationState>(OperationState.Waiting));
        var snapshot = Snapshot();

        var result = ConditionalDependencyEvaluator.Evaluate([dependency], snapshot);

        Assert.AreEqual(ConditionalDependencyBlockReason.MissingPredecessorState, result.Blocked[0].Reason);
        Assert.IsFalse(result.Blocked[0].ObservedState.HasValue);
    }

    private static StateSnapshot<OperationId, OperationState> Snapshot(params (int Id, OperationState State)[] values)
        => new(values.ToDictionary(static value => new OperationId(value.Id), static value => value.State));

    private readonly record struct OperationId(int Value);

    private enum OperationState
    {
        Waiting,
        InProgress,
        Completed
    }

    private sealed class AtLeastCondition(OperationState required) : IStateCondition<OperationState>
    {
        public bool IsSatisfied(OperationState state) => state >= required;
    }
}
