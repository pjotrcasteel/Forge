using System.Diagnostics;
using Forge.Decide.OpenTelemetry;

namespace Forge.Decide.OpenTelemetry.Tests;

[TestClass]
public sealed class StrategyTelemetryExtensionsTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task DecideWithOpenTelemetryAsync_EmitsDecisionStrategyAndSelectionActivities()
    {
        var stopped = new List<Activity>();
        using var listener = CreateListener(stopped);
        ActivitySource.AddActivityListener(listener);

        var space = StrategySpace<TelemetrySpace>
            .Define<RequestContext, Plan>("telemetry")
            .Add(new FixedStrategy().WithOpenTelemetry())
            .SelectWith(StrategySelectionPolicies.ExactlyOne<RequestContext, Plan>().WithOpenTelemetry())
            .Build();

        var decision = await space.DecideWithOpenTelemetryAsync(new RequestContext(), TestContext.CancellationToken);

        Assert.AreEqual("fixed", decision.SelectedStrategyId.Value);
        Assert.IsTrue(stopped.Any(activity => activity.OperationName == "forge.decide.decide"));
        Assert.IsTrue(stopped.Any(activity => activity.OperationName == "forge.decide.strategy.propose"));
        Assert.IsTrue(stopped.Any(activity => activity.OperationName == "forge.decide.selection"));
    }

    [TestMethod]
    public async Task DecideWithShadowOpenTelemetryAsync_EmitsSelectionChangeTag()
    {
        var stopped = new List<Activity>();
        using var listener = CreateListener(stopped);
        ActivitySource.AddActivityListener(listener);

        var space = StrategySpace<TelemetrySpace>
            .Define<RequestContext, Plan>("telemetry")
            .Add(new NamedStrategy("cheap", 10))
            .Add(new NamedStrategy("strong", 20))
            .Build();
        var context = new RequestContext();
        var comparison = await space.CompareAsync(context, TestContext.CancellationToken);

        var shadow = await comparison.DecideWithShadowOpenTelemetryAsync(
            context,
            StrategySelectionPolicies.LowestBy<RequestContext, Plan, int>(static (_, candidate) => candidate.Plan.Score),
            StrategySelectionPolicies.HighestBy<RequestContext, Plan, int>(static (_, candidate) => candidate.Plan.Score),
            TestContext.CancellationToken);

        var activity = stopped.Single(item => item.OperationName == "forge.decide.shadow");
        Assert.IsTrue(shadow.SelectionChanged);
        Assert.AreEqual(true, activity.GetTagItem("forge.decide.selection_changed"));
    }

    private static ActivityListener CreateListener(List<Activity> stopped)
    {
        return new ActivityListener
        {
            ShouldListenTo = static source => source.Name == ForgeDecideTelemetry.ActivitySourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = stopped.Add,
        };
    }

    private sealed class TelemetrySpace
    {
    }

    private sealed record RequestContext;

    private sealed record Plan(int Score);

    private sealed class FixedStrategy : IStrategy<RequestContext, Plan>
    {
        public StrategyId Id => "fixed";

        public ValueTask<StrategyProposal<Plan>> ProposeAsync(RequestContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(StrategyProposal<Plan>.Applicable(new Plan(10)));
        }
    }

    private sealed class NamedStrategy : IStrategy<RequestContext, Plan>
    {
        private readonly Plan _plan;

        public NamedStrategy(string id, int score)
        {
            Id = id;
            _plan = new Plan(score);
        }

        public StrategyId Id { get; }

        public ValueTask<StrategyProposal<Plan>> ProposeAsync(RequestContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(StrategyProposal<Plan>.Applicable(_plan));
        }
    }
}