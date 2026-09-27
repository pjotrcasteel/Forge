namespace Forge.Sync.Tests;

[TestClass]
public sealed class ReadinessEvaluatorTests
{
    [TestMethod]
    public void Evaluate_ShouldAccumulateTypedReasonsAndKeepRunnableItemsInInputOrder()
    {
        Operation[] operations =
        [
            new(new OperationId(1), false),
            new(new OperationId(2), true),
            new(new OperationId(3), false)
        ];
        var provider = new RequirementProvider();

        var result = ReadinessEvaluator.Evaluate(operations, static item => item.Id, new Context(false), provider);

        CollectionAssert.AreEqual(
            new[] { new OperationId(1), new OperationId(3) },
            result.Runnable.Select(static item => item.Key).ToArray());
        Assert.AreEqual(1, result.Blocked.Count);
        Assert.AreEqual(new OperationId(2), result.Blocked[0].Key);
        Assert.AreEqual(BlockReason.PlanPaused, result.Blocked[0].Reasons.Single());
    }

    [TestMethod]
    public void ConditionalDependencyRequirement_ShouldComposeWithGeneralReadinessModel()
    {
        var dependency = new ConditionalDependency<OperationId, OperationState>(
            new OperationId(1),
            new OperationId(2),
            new StateEqualsCondition<OperationState>(OperationState.Completed));
        var requirement = new ConditionalDependencyReadinessRequirement<OperationId, OperationState, BlockReason>(
            dependency,
            static _ => BlockReason.PredecessorNotCompleted);
        var provider = new SingleRequirementProvider(requirement);
        var states = new StateSnapshot<OperationId, OperationState>(
            new Dictionary<OperationId, OperationState> { [new OperationId(1)] = OperationState.InProgress });
        Operation[] items = [new(new OperationId(2), false)];

        var result = ReadinessEvaluator.Evaluate(items, static item => item.Id, states, provider);

        Assert.AreEqual(BlockReason.PredecessorNotCompleted, result.Blocked.Single().Reasons.Single());
    }

    private sealed record Operation(OperationId Id, bool RequiresRunningPlan);
    private readonly record struct OperationId(int Value);
    private sealed record Context(bool PlanRunning);

    private enum OperationState
    {
        InProgress,
        Completed
    }

    private enum BlockReason
    {
        PlanPaused,
        PredecessorNotCompleted
    }

    private sealed class RequirementProvider : IReadinessRequirementProvider<Operation, Context, BlockReason>
    {
        private static readonly IReadinessRequirement<Context, BlockReason>[] s_paused = [new PlanRunningRequirement()];

        public IReadOnlyList<IReadinessRequirement<Context, BlockReason>> GetRequirements(Operation item)
            => item.RequiresRunningPlan ? s_paused : [];
    }

    private sealed class PlanRunningRequirement : IReadinessRequirement<Context, BlockReason>
    {
        public ReadinessRequirementResult<BlockReason> Evaluate(Context context)
            => context.PlanRunning
                ? ReadinessRequirementResult<BlockReason>.Satisfied()
                : ReadinessRequirementResult<BlockReason>.Blocked(BlockReason.PlanPaused);
    }

    private sealed class SingleRequirementProvider(
        IReadinessRequirement<StateSnapshot<OperationId, OperationState>, BlockReason> requirement)
        : IReadinessRequirementProvider<Operation, StateSnapshot<OperationId, OperationState>, BlockReason>
    {
        public IReadOnlyList<IReadinessRequirement<StateSnapshot<OperationId, OperationState>, BlockReason>> GetRequirements(
            Operation item)
            => [requirement];
    }
}
