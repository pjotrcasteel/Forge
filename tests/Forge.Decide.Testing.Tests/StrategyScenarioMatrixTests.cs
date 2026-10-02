using Forge.Decide;
using Forge.Decide.Testing;

namespace Forge.Decide.Testing.Tests;

[TestClass]
public sealed class StrategyScenarioMatrixTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task EvaluateAsync_ReportsSelectedUncoveredAndAmbiguousScenarios()
    {
        var space = StrategySpace<TestSpace>
            .Define<TestContextModel, TestPlan>("test")
            .Add(new LowStrategy())
            .Add(new HighStrategy())
            .Build();
        var scenarios = new[]
        {
            new StrategyScenario<TestContextModel>("low-only", new TestContextModel(5)),
            new StrategyScenario<TestContextModel>("gap", new TestContextModel(0)),
            new StrategyScenario<TestContextModel>("overlap", new TestContextModel(20)),
        };

        var result = await StrategyScenarioMatrix.EvaluateAsync(space, scenarios, TestContext.CancellationToken);

        Assert.AreEqual(StrategyScenarioStatus.Selected, result.Scenarios[0].Status);
        Assert.AreEqual("low", result.Scenarios[0].SelectedStrategyId?.Value);
        Assert.AreEqual(StrategyScenarioStatus.NoApplicableStrategy, result.Scenarios[1].Status);
        Assert.AreEqual(StrategyScenarioStatus.Ambiguous, result.Scenarios[2].Status);
        CollectionAssert.AreEqual(
            new[] { "low", "high" },
            result.Scenarios[2].AmbiguousStrategyIds.Select(strategyId => strategyId.Value).ToArray());
        Assert.AreEqual(1, result.Uncovered.Count);
        Assert.AreEqual(1, result.Ambiguous.Count);
        Assert.IsFalse(result.IsFullyResolved);
    }

    [TestMethod]
    public async Task EvaluateAsync_EvaluatesStrategiesOncePerScenario()
    {
        var strategy = new CountingStrategy();
        var space = StrategySpace<TestSpace>
            .Define<TestContextModel, TestPlan>("test")
            .Add(strategy)
            .Build();
        var scenarios = new[]
        {
            new StrategyScenario<TestContextModel>("one", new TestContextModel(1)),
            new StrategyScenario<TestContextModel>("two", new TestContextModel(2)),
        };

        await StrategyScenarioMatrix.EvaluateAsync(space, scenarios, TestContext.CancellationToken);

        Assert.AreEqual(2, strategy.CallCount);
    }

    [TestMethod]
    public async Task EvaluateAsync_RejectsDuplicateScenarioNames()
    {
        var space = StrategySpace<TestSpace>
            .Define<TestContextModel, TestPlan>("test")
            .Add(new CountingStrategy())
            .Build();
        var scenarios = new[]
        {
            new StrategyScenario<TestContextModel>("duplicate", new TestContextModel(1)),
            new StrategyScenario<TestContextModel>("duplicate", new TestContextModel(2)),
        };

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await StrategyScenarioMatrix.EvaluateAsync(space, scenarios, TestContext.CancellationToken));
    }

    private sealed class TestSpace
    {
    }

    private sealed record TestContextModel(int Value);

    private sealed record TestPlan(string Name);

    private sealed class LowStrategy : IStrategy<TestContextModel, TestPlan>
    {
        public StrategyId Id => "low";

        public ValueTask<StrategyProposal<TestPlan>> ProposeAsync(TestContextModel context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(context.Value > 0
                ? StrategyProposal<TestPlan>.Applicable(new TestPlan("low"), "Positive value.")
                : StrategyProposal<TestPlan>.NotApplicable("Value must be positive."));
        }
    }

    private sealed class HighStrategy : IStrategy<TestContextModel, TestPlan>
    {
        public StrategyId Id => "high";

        public ValueTask<StrategyProposal<TestPlan>> ProposeAsync(TestContextModel context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(context.Value >= 10
                ? StrategyProposal<TestPlan>.Applicable(new TestPlan("high"), "High value.")
                : StrategyProposal<TestPlan>.NotApplicable("Value must be at least ten."));
        }
    }

    private sealed class CountingStrategy : IStrategy<TestContextModel, TestPlan>
    {
        public StrategyId Id => "counting";

        public int CallCount { get; private set; }

        public ValueTask<StrategyProposal<TestPlan>> ProposeAsync(TestContextModel context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(StrategyProposal<TestPlan>.Applicable(new TestPlan(context.Value.ToString())));
        }
    }
}