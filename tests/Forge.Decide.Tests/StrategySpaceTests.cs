using Forge.Decide;

namespace Forge.Decide.Tests;

[TestClass]
public sealed class StrategySpaceTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task CompareAsync_EvaluatesOnlyStrategiesAdmittedToSpace()
    {
        var fast = TestStrategy.Applicable("fast", "fast-plan");
        var balanced = TestStrategy.Applicable("balanced", "balanced-plan");
        var batchOnly = TestStrategy.Applicable("batch-only", "batch-plan");

        var space = StrategySpace<InteractiveSpace>
            .Define<TestContextModel, TestPlan>("interactive")
            .Add(fast)
            .Add(balanced)
            .Build();

        var comparison = await space.CompareAsync(new TestContextModel(), TestContext.CancellationToken);

        Assert.AreEqual(1, fast.CallCount);
        Assert.AreEqual(1, balanced.CallCount);
        Assert.AreEqual(0, batchOnly.CallCount);
        CollectionAssert.AreEqual(
            new[] { "fast", "balanced" },
            comparison.Candidates.Select(candidate => candidate.StrategyId.Value).ToArray());
    }

    [TestMethod]
    public async Task CompareAsync_PreservesApplicableAndRejectedExplanations()
    {
        var space = StrategySpace<InteractiveSpace>
            .Define<TestContextModel, TestPlan>("interactive")
            .Add(TestStrategy.Applicable("fast", "fast-plan", "Meets the latency target."))
            .Add(TestStrategy.Rejected("balanced", "Capacity threshold was not met."))
            .Build();

        var comparison = await space.CompareAsync(new TestContextModel(), TestContext.CancellationToken);

        var applicable = (ApplicableStrategyCandidate<TestPlan>)comparison.Candidates[0];
        var rejected = (RejectedStrategyCandidate<TestPlan>)comparison.Candidates[1];

        Assert.AreEqual("Meets the latency target.", applicable.Reason);
        Assert.AreEqual("Capacity threshold was not met.", rejected.Reason);
    }

    [TestMethod]
    public async Task DecideAsync_DefaultPolicy_SelectsTheOnlyApplicableProposal()
    {
        var space = StrategySpace<InteractiveSpace>
            .Define<TestContextModel, TestPlan>("interactive")
            .Add(TestStrategy.Applicable("fast", "fast-plan"))
            .Add(TestStrategy.Rejected("balanced", "Not applicable."))
            .Build();

        var decision = await space.DecideAsync(new TestContextModel(), TestContext.CancellationToken);

        Assert.AreEqual("fast", decision.SelectedStrategyId.Value);
        Assert.AreEqual("fast-plan", decision.Plan.Name);
        Assert.AreEqual(2, decision.Candidates.Count);
    }

    [TestMethod]
    public async Task DecideAsync_DefaultPolicy_ThrowsWhenMultipleProposalsAreApplicable()
    {
        var space = StrategySpace<InteractiveSpace>
            .Define<TestContextModel, TestPlan>("interactive")
            .Add(TestStrategy.Applicable("fast", "fast-plan"))
            .Add(TestStrategy.Applicable("balanced", "balanced-plan"))
            .Build();

        var exception = await Assert.ThrowsExactlyAsync<AmbiguousStrategyDecisionException>(
            async () => await space.DecideAsync(new TestContextModel(), TestContext.CancellationToken));

        CollectionAssert.AreEqual(
            new[] { "fast", "balanced" },
            exception.StrategyIds.Select(strategyId => strategyId.Value).ToArray());
    }

    [TestMethod]
    public async Task DecideAsync_ThrowsWhenNoProposalIsApplicable()
    {
        var space = StrategySpace<InteractiveSpace>
            .Define<TestContextModel, TestPlan>("interactive")
            .Add(TestStrategy.Rejected("fast", "Not applicable."))
            .Add(TestStrategy.Rejected("balanced", "Not applicable."))
            .Build();

        var exception = await Assert.ThrowsExactlyAsync<NoApplicableStrategyException>(
            async () => await space.DecideAsync(new TestContextModel(), TestContext.CancellationToken));

        Assert.AreEqual("interactive", exception.SpaceId.Value);
    }

    [TestMethod]
    public async Task DecideAsync_CustomPolicyCanSelectBetweenApplicableProposals()
    {
        var space = StrategySpace<InteractiveSpace>
            .Define<TestContextModel, TestPlan>("interactive")
            .Add(TestStrategy.Applicable("fast", "fast-plan"))
            .Add(TestStrategy.Applicable("balanced", "balanced-plan"))
            .SelectWith(new PreferStrategyPolicy("balanced"))
            .Build();

        var decision = await space.DecideAsync(new TestContextModel(), TestContext.CancellationToken);

        Assert.AreEqual("balanced", decision.SelectedStrategyId.Value);
        Assert.AreEqual("balanced-plan", decision.Plan.Name);
    }

    [TestMethod]
    public void Add_RejectsDuplicateStrategyIdsInsideOneSpace()
    {
        var builder = StrategySpace<InteractiveSpace>
            .Define<TestContextModel, TestPlan>("interactive")
            .Add(TestStrategy.Applicable("fast", "first-plan"));

        Assert.ThrowsExactly<ArgumentException>(
            () => builder.Add(TestStrategy.Applicable("fast", "second-plan")));
    }

    [TestMethod]
    public async Task CompareAsync_ForwardsCancellationTokenToStrategies()
    {
        var strategy = TestStrategy.Applicable("fast", "fast-plan");
        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);

        var space = StrategySpace<InteractiveSpace>
            .Define<TestContextModel, TestPlan>("interactive")
            .Add(strategy)
            .Build();

        await space.CompareAsync(new TestContextModel(), cancellationTokenSource.Token);

        Assert.AreEqual(cancellationTokenSource.Token, strategy.LastCancellationToken);
    }

    [TestMethod]
    public void TypedSpacesCanUseTheSameContextAndPlanWithoutSharingCandidates()
    {
        var shared = TestStrategy.Applicable("shared", "shared-plan");
        var interactiveOnly = TestStrategy.Applicable("interactive-only", "interactive-plan");
        var batchOnly = TestStrategy.Applicable("batch-only", "batch-plan");

        StrategySpace<InteractiveSpace, TestContextModel, TestPlan> interactive = StrategySpace<InteractiveSpace>
            .Define<TestContextModel, TestPlan>("interactive")
            .Add(shared)
            .Add(interactiveOnly)
            .Build();

        StrategySpace<BatchSpace, TestContextModel, TestPlan> batch = StrategySpace<BatchSpace>
            .Define<TestContextModel, TestPlan>("batch")
            .Add(shared)
            .Add(batchOnly)
            .Build();

        Assert.AreEqual("interactive", interactive.Id.Value);
        Assert.AreEqual("batch", batch.Id.Value);
    }

    private sealed class InteractiveSpace
    {
    }

    private sealed class BatchSpace
    {
    }

    private sealed record TestContextModel
    {
    }

    private sealed record TestPlan(string Name);

    private sealed class TestStrategy : IStrategy<TestContextModel, TestPlan>
    {
        private readonly StrategyProposal<TestPlan> _proposal;

        private TestStrategy(string id, StrategyProposal<TestPlan> proposal)
        {
            Id = id;
            _proposal = proposal;
        }

        public StrategyId Id { get; }

        public int CallCount { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public static TestStrategy Applicable(string id, string plan, string? reason = null) =>
            new(id, StrategyProposal<TestPlan>.Applicable(new TestPlan(plan), reason));

        public static TestStrategy Rejected(string id, string reason) =>
            new(id, StrategyProposal<TestPlan>.NotApplicable(reason));

        public ValueTask<StrategyProposal<TestPlan>> ProposeAsync(TestContextModel context, CancellationToken cancellationToken)
        {
            CallCount++;
            LastCancellationToken = cancellationToken;
            return ValueTask.FromResult(_proposal);
        }
    }

    private sealed class PreferStrategyPolicy : IStrategySelectionPolicy<TestContextModel, TestPlan>
    {
        private readonly StrategyId _preferred;

        public PreferStrategyPolicy(StrategyId preferred)
        {
            _preferred = preferred;
        }

        public ValueTask<ApplicableStrategyCandidate<TestPlan>> SelectAsync(
            TestContextModel context,
            IReadOnlyList<ApplicableStrategyCandidate<TestPlan>> candidates,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(candidates.Single(candidate => candidate.StrategyId == _preferred));
        }
    }
}