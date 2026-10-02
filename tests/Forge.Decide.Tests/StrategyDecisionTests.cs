using Forge.Decide;

namespace Forge.Decide.Tests;

[TestClass]
public sealed class StrategyDecisionTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task Comparison_CanApplyTwoPoliciesWithoutReevaluatingStrategies()
    {
        var fast = new ScoredStrategy("fast", 10);
        var safe = new ScoredStrategy("safe", 20);
        var space = CreateSpace(fast, safe);
        var context = new DecisionContext();

        var comparison = await space.CompareAsync(context, TestContext.CancellationToken);
        var highest = await comparison.DecideAsync(
            context,
            StrategySelectionPolicies.HighestBy<DecisionContext, Plan, int>((_, candidate) => candidate.Plan.Score),
            TestContext.CancellationToken);
        var lowest = await comparison.DecideAsync(
            context,
            StrategySelectionPolicies.LowestBy<DecisionContext, Plan, int>((_, candidate) => candidate.Plan.Score),
            TestContext.CancellationToken);

        Assert.AreEqual("safe", highest.SelectedStrategyId.Value);
        Assert.AreEqual("fast", lowest.SelectedStrategyId.Value);
        Assert.AreEqual(1, fast.CallCount);
        Assert.AreEqual(1, safe.CallCount);
    }

    [TestMethod]
    public async Task HighestBy_ThrowsOnTieInsteadOfUsingRegistrationOrder()
    {
        var space = CreateSpace(new ScoredStrategy("first", 10), new ScoredStrategy("second", 10));
        var context = new DecisionContext();
        var comparison = await space.CompareAsync(context, TestContext.CancellationToken);

        var exception = await Assert.ThrowsExactlyAsync<AmbiguousStrategyDecisionException>(
            async () => await comparison.DecideAsync(
                context,
                StrategySelectionPolicies.HighestBy<DecisionContext, Plan, int>((_, candidate) => candidate.Plan.Score),
                TestContext.CancellationToken));

        CollectionAssert.AreEqual(new[] { "first", "second" }, exception.StrategyIds.Select(id => id.Value).ToArray());
    }

    [TestMethod]
    public async Task Explain_PreservesSelectedApplicableAndRejectedEvidence()
    {
        var space = StrategySpace<DecisionSpace>
            .Define<DecisionContext, Plan>("decision")
            .Add(new ScoredStrategy("fast", 10, "Fast proposal."))
            .Add(new ScoredStrategy("safe", 20, "Safe proposal."))
            .Add(new RejectedStrategy("legacy", "Capability unavailable."))
            .SelectWith(StrategySelectionPolicies.HighestBy<DecisionContext, Plan, int>((_, candidate) => candidate.Plan.Score))
            .Build();

        var decision = await space.DecideAsync(new DecisionContext(), TestContext.CancellationToken);
        var explanation = decision.Explain();

        Assert.AreEqual("decision", explanation.SpaceId.Value);
        Assert.AreEqual("safe", explanation.SelectedStrategyId.Value);
        Assert.AreEqual(StrategyCandidateDisposition.Applicable, explanation.Candidates[0].Disposition);
        Assert.AreEqual(StrategyCandidateDisposition.Selected, explanation.Candidates[1].Disposition);
        Assert.AreEqual(StrategyCandidateDisposition.Rejected, explanation.Candidates[2].Disposition);
        StringAssert.Contains(explanation.ToString(), "Capability unavailable.");
    }

    [TestMethod]
    public async Task Digest_IsStableForEquivalentDecisionEvidence()
    {
        var context = new DecisionContext();
        var first = await CreateSingleDecisionAsync(context);
        var second = await CreateSingleDecisionAsync(context);

        var firstDigest = StrategyDecisionDigest.ComputeSha256Hex(first, static plan => $"{plan.Name}|{plan.Score}");
        var secondDigest = StrategyDecisionDigest.ComputeSha256Hex(second, static plan => $"{plan.Name}|{plan.Score}");

        Assert.AreEqual(firstDigest, secondDigest);
        Assert.AreEqual(64, firstDigest.Length);
    }

    [TestMethod]
    public async Task Digest_ChangesWhenCandidateEvidenceChanges()
    {
        var context = new DecisionContext();
        var firstSpace = StrategySpace<DecisionSpace>
            .Define<DecisionContext, Plan>("decision")
            .Add(new ScoredStrategy("fast", 10, "Original reason."))
            .Build();
        var secondSpace = StrategySpace<DecisionSpace>
            .Define<DecisionContext, Plan>("decision")
            .Add(new ScoredStrategy("fast", 10, "Changed reason."))
            .Build();

        var first = await firstSpace.DecideAsync(context, TestContext.CancellationToken);
        var second = await secondSpace.DecideAsync(context, TestContext.CancellationToken);

        var firstDigest = StrategyDecisionDigest.ComputeSha256Hex(first, static plan => $"{plan.Name}|{plan.Score}");
        var secondDigest = StrategyDecisionDigest.ComputeSha256Hex(second, static plan => $"{plan.Name}|{plan.Score}");

        Assert.AreNotEqual(firstDigest, secondDigest);
    }

    private static StrategySpace<DecisionSpace, DecisionContext, Plan> CreateSpace(params ScoredStrategy[] strategies)
    {
        var builder = StrategySpace<DecisionSpace>.Define<DecisionContext, Plan>("decision");

        foreach (var strategy in strategies)
        {
            builder.Add(strategy);
        }

        return builder.Build();
    }

    private async Task<StrategyDecision<DecisionSpace, Plan>> CreateSingleDecisionAsync(DecisionContext context)
    {
        var space = StrategySpace<DecisionSpace>
            .Define<DecisionContext, Plan>("decision")
            .Add(new ScoredStrategy("fast", 10, "Fast proposal."))
            .Build();

        return await space.DecideAsync(context, TestContext.CancellationToken);
    }

    private sealed class DecisionSpace
    {
    }

    private sealed record DecisionContext;

    private sealed record Plan(string Name, int Score);

    private sealed class ScoredStrategy : IStrategy<DecisionContext, Plan>
    {
        private readonly StrategyProposal<Plan> _proposal;

        public ScoredStrategy(string id, int score, string? reason = null)
        {
            Id = id;
            _proposal = StrategyProposal<Plan>.Applicable(new Plan(id, score), reason);
        }

        public StrategyId Id { get; }

        public int CallCount { get; private set; }

        public ValueTask<StrategyProposal<Plan>> ProposeAsync(DecisionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(_proposal);
        }
    }

    private sealed class RejectedStrategy : IStrategy<DecisionContext, Plan>
    {
        private readonly string _reason;

        public RejectedStrategy(string id, string reason)
        {
            Id = id;
            _reason = reason;
        }

        public StrategyId Id { get; }

        public ValueTask<StrategyProposal<Plan>> ProposeAsync(DecisionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(StrategyProposal<Plan>.NotApplicable(_reason));
        }
    }
}