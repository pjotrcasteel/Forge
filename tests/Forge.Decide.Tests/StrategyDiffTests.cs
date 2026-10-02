using Forge.Decide;

namespace Forge.Decide.Tests;

[TestClass]
public sealed class StrategyDiffTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task DecideWithShadowAsync_UsesTheSameEvaluatedCandidates()
    {
        var low = new TestStrategy("low", StrategyProposal<TestPlan>.Applicable(new TestPlan(10), "Low plan."));
        var high = new TestStrategy("high", StrategyProposal<TestPlan>.Applicable(new TestPlan(20), "High plan."));
        var space = CreateSpace(low, high);
        var comparison = await space.CompareAsync(new TestContextModel(), TestContext.CancellationToken);

        var result = await comparison.DecideWithShadowAsync(
            new TestContextModel(),
            StrategySelectionPolicies.LowestBy<TestContextModel, TestPlan, int>((_, candidate) => candidate.Plan.Score),
            StrategySelectionPolicies.HighestBy<TestContextModel, TestPlan, int>((_, candidate) => candidate.Plan.Score),
            TestContext.CancellationToken);

        Assert.AreEqual("low", result.Production.SelectedStrategyId.Value);
        Assert.AreEqual("high", result.Shadow.SelectedStrategyId.Value);
        Assert.IsTrue(result.SelectionChanged);
        Assert.AreSame(result.Production.Candidates, result.Shadow.Candidates);
        Assert.AreEqual(1, low.CallCount);
        Assert.AreEqual(1, high.CallCount);
    }

    [TestMethod]
    public async Task ComparisonDiff_DetectsApplicabilityReasonAndPlanChanges()
    {
        var before = await CreateSpace(
                new TestStrategy("stable", StrategyProposal<TestPlan>.Applicable(new TestPlan(10), "Same.")),
                new TestStrategy("changed", StrategyProposal<TestPlan>.Applicable(new TestPlan(20), "Before.")),
                new TestStrategy("removed", StrategyProposal<TestPlan>.NotApplicable("Removed.")))
            .CompareAsync(new TestContextModel(), TestContext.CancellationToken);
        var after = await CreateSpace(
                new TestStrategy("stable", StrategyProposal<TestPlan>.Applicable(new TestPlan(10), "Same.")),
                new TestStrategy("changed", StrategyProposal<TestPlan>.NotApplicable("After.")),
                new TestStrategy("added", StrategyProposal<TestPlan>.Applicable(new TestPlan(30), "Added.")))
            .CompareAsync(new TestContextModel(), TestContext.CancellationToken);

        var diff = StrategyComparisonDiff.Between(before, after, static plan => plan.Score.ToString(System.Globalization.CultureInfo.InvariantCulture));

        Assert.IsTrue(diff.HasChanges);
        Assert.AreEqual(StrategyCandidateChange.None, diff.Candidates[0].Changes);
        Assert.AreEqual(
            StrategyCandidateChange.ApplicabilityChanged | StrategyCandidateChange.ReasonChanged,
            diff.Candidates[1].Changes);
        Assert.AreEqual(StrategyCandidateChange.Removed, diff.Candidates[2].Changes);
        Assert.AreEqual(StrategyCandidateChange.Added, diff.Candidates[3].Changes);
    }

    [TestMethod]
    public async Task ComparisonDiff_DetectsChangedApplicablePlan()
    {
        var before = await CreateSpace(
                new TestStrategy("strategy", StrategyProposal<TestPlan>.Applicable(new TestPlan(10), "Same.")))
            .CompareAsync(new TestContextModel(), TestContext.CancellationToken);
        var after = await CreateSpace(
                new TestStrategy("strategy", StrategyProposal<TestPlan>.Applicable(new TestPlan(20), "Same.")))
            .CompareAsync(new TestContextModel(), TestContext.CancellationToken);

        var diff = StrategyComparisonDiff.Between(before, after, static plan => plan.Score.ToString(System.Globalization.CultureInfo.InvariantCulture));

        Assert.AreEqual(StrategyCandidateChange.PlanChanged, diff.Candidates[0].Changes);
    }

    [TestMethod]
    public async Task DecisionDiff_DetectsSelectionChange()
    {
        var beforeSpace = CreateSpace(
            new TestStrategy("first", StrategyProposal<TestPlan>.Applicable(new TestPlan(10), "First.")),
            new TestStrategy("second", StrategyProposal<TestPlan>.Applicable(new TestPlan(20), "Second.")));
        var afterSpace = CreateSpace(
            new TestStrategy("first", StrategyProposal<TestPlan>.Applicable(new TestPlan(30), "First.")),
            new TestStrategy("second", StrategyProposal<TestPlan>.Applicable(new TestPlan(20), "Second.")));
        var policy = StrategySelectionPolicies.LowestBy<TestContextModel, TestPlan, int>((_, candidate) => candidate.Plan.Score);
        var context = new TestContextModel();
        var beforeComparison = await beforeSpace.CompareAsync(context, TestContext.CancellationToken);
        var afterComparison = await afterSpace.CompareAsync(context, TestContext.CancellationToken);
        var before = await beforeComparison.DecideAsync(context, policy, TestContext.CancellationToken);
        var after = await afterComparison.DecideAsync(context, policy, TestContext.CancellationToken);

        var diff = StrategyDecisionDiff.Between(
            before,
            after,
            static plan => plan.Score.ToString(System.Globalization.CultureInfo.InvariantCulture));

        Assert.IsTrue(diff.SelectionChanged);
        Assert.IsTrue(diff.HasChanges);
        Assert.AreEqual("first", diff.BeforeSelectedStrategyId.Value);
        Assert.AreEqual("second", diff.AfterSelectedStrategyId.Value);
        Assert.IsTrue(diff.Evidence.Candidates[0].Changes.HasFlag(StrategyCandidateChange.PlanChanged));
    }

    private static StrategySpace<TestSpace, TestContextModel, TestPlan> CreateSpace(params TestStrategy[] strategies)
    {
        var builder = StrategySpace<TestSpace>.Define<TestContextModel, TestPlan>("test");

        foreach (var strategy in strategies)
        {
            builder.Add(strategy);
        }

        return builder.Build();
    }

    private sealed class TestSpace
    {
    }

    private sealed record TestContextModel;

    private sealed record TestPlan(int Score);

    private sealed class TestStrategy : IStrategy<TestContextModel, TestPlan>
    {
        private readonly StrategyProposal<TestPlan> _proposal;

        public TestStrategy(string id, StrategyProposal<TestPlan> proposal)
        {
            Id = id;
            _proposal = proposal;
        }

        public StrategyId Id { get; }

        public int CallCount { get; private set; }

        public ValueTask<StrategyProposal<TestPlan>> ProposeAsync(TestContextModel context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(_proposal);
        }
    }
}