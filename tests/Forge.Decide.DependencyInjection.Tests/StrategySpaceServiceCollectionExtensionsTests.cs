using Forge.Decide.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Forge.Decide.DependencyInjection.Tests;

[TestClass]
public sealed class StrategySpaceServiceCollectionExtensionsTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task AddForgeDecisionSpace_ResolvesTypedSpaceAndApplicationDependencies()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new StrategySettings(EstimatedCost: 25));
        services.AddForgeDecisionSpace<InteractiveSpace, RequestContext, Plan>(
            "interactive",
            space => space.Add<ConfiguredStrategy>());

        await using var provider = services.BuildServiceProvider();
        var space = provider.GetRequiredService<StrategySpace<InteractiveSpace, RequestContext, Plan>>();

        var decision = await space.DecideAsync(new RequestContext(), TestContext.CancellationToken);

        Assert.AreEqual("configured", decision.SelectedStrategyId.Value);
        Assert.AreEqual(25, decision.Plan.EstimatedCost);
    }

    [TestMethod]
    public async Task AddForgeDecisionSpace_DoesNotAdmitStrategiesFromAnotherSpace()
    {
        var services = new ServiceCollection();
        var batchOnly = new BatchOnlyStrategy();
        services.AddSingleton(batchOnly);
        services.AddForgeDecisionSpace<InteractiveSpace, RequestContext, Plan>(
            "interactive",
            space => space.Add<ConfiguredStrategy>());
        services.AddForgeDecisionSpace<BatchSpace, RequestContext, Plan>(
            "batch",
            space => space.Add<BatchOnlyStrategy>());
        services.AddSingleton(new StrategySettings(EstimatedCost: 25));

        await using var provider = services.BuildServiceProvider();
        var interactive = provider.GetRequiredService<StrategySpace<InteractiveSpace, RequestContext, Plan>>();

        await interactive.CompareAsync(new RequestContext(), TestContext.CancellationToken);

        Assert.AreEqual(0, batchOnly.EvaluationCount);
    }

    [TestMethod]
    public async Task AddForgeDecisionSpace_UsesApplicationOwnedSelectionPolicyFactory()
    {
        var services = new ServiceCollection();
        services.AddForgeDecisionSpace<InteractiveSpace, RequestContext, Plan>(
            "interactive",
            space => space
                .Add(_ => new FixedStrategy("expensive", 100))
                .Add(_ => new FixedStrategy("cheap", 20))
                .SelectWith(_ => StrategySelectionPolicies.LowestBy<RequestContext, Plan, int>(
                    static (_, candidate) => candidate.Plan.EstimatedCost)));

        await using var provider = services.BuildServiceProvider();
        var space = provider.GetRequiredService<StrategySpace<InteractiveSpace, RequestContext, Plan>>();

        var decision = await space.DecideAsync(new RequestContext(), TestContext.CancellationToken);

        Assert.AreEqual("cheap", decision.SelectedStrategyId.Value);
    }

    private sealed class InteractiveSpace
    {
    }

    private sealed class BatchSpace
    {
    }

    private sealed record RequestContext;

    private sealed record Plan(int EstimatedCost);

    private sealed record StrategySettings(int EstimatedCost);

    private sealed class ConfiguredStrategy : IStrategy<RequestContext, Plan>
    {
        private readonly StrategySettings _settings;

        public ConfiguredStrategy(StrategySettings settings)
        {
            _settings = settings;
        }

        public StrategyId Id => "configured";

        public ValueTask<StrategyProposal<Plan>> ProposeAsync(RequestContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(StrategyProposal<Plan>.Applicable(new Plan(_settings.EstimatedCost)));
        }
    }

    private sealed class BatchOnlyStrategy : IStrategy<RequestContext, Plan>
    {
        public StrategyId Id => "batch-only";

        public int EvaluationCount { get; private set; }

        public ValueTask<StrategyProposal<Plan>> ProposeAsync(RequestContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EvaluationCount++;
            return ValueTask.FromResult(StrategyProposal<Plan>.Applicable(new Plan(10)));
        }
    }

    private sealed class FixedStrategy : IStrategy<RequestContext, Plan>
    {
        private readonly Plan _plan;

        public FixedStrategy(string id, int estimatedCost)
        {
            Id = id;
            _plan = new Plan(estimatedCost);
        }

        public StrategyId Id { get; }

        public ValueTask<StrategyProposal<Plan>> ProposeAsync(RequestContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(StrategyProposal<Plan>.Applicable(_plan));
        }
    }
}