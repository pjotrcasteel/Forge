namespace Forge.Sync.Tests;

[TestClass]
public sealed class ScenarioMatrixTests
{
    [TestMethod]
    public void Cross_ShouldEnumerateCartesianProductDeterministically()
    {
        var left = new ScenarioAxis<NetworkCaseId, NetworkOutcome>(
        [
            new(new NetworkCaseId(1), NetworkOutcome.Success),
            new(new NetworkCaseId(2), NetworkOutcome.Failure)
        ]);
        var right = new ScenarioAxis<InventoryCaseId, InventoryTiming>(
        [
            new(new InventoryCaseId(1), InventoryTiming.Immediate),
            new(new InventoryCaseId(2), InventoryTiming.Delayed)
        ]);

        var combinations = ScenarioMatrix.Cross(left, right).ToArray();

        Assert.AreEqual(4, combinations.Length);
        Assert.AreEqual(NetworkOutcome.Success, combinations[0].Left.Value);
        Assert.AreEqual(InventoryTiming.Immediate, combinations[0].Right.Value);
        Assert.AreEqual(NetworkOutcome.Failure, combinations[3].Left.Value);
        Assert.AreEqual(InventoryTiming.Delayed, combinations[3].Right.Value);
    }

    [TestMethod]
    public void Simulate_ShouldRemainLazyUntilResultsAreRequested()
    {
        var evaluator = new CountingEvaluator();
        var simulations = ScenarioSimulator.Simulate(new[] { 1, 2, 3 }, evaluator);

        Assert.AreEqual(0, evaluator.Count);
        var first = simulations.First();

        Assert.AreEqual(1, evaluator.Count);
        Assert.AreEqual(2, first.Result);
    }

    [TestMethod]
    public async Task SimulateAsync_ShouldPassCancellationTokenToEveryScenario()
    {
        using var source = new CancellationTokenSource();
        var evaluator = new CancellingEvaluator(source);
        var observed = new List<int>();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var result in ScenarioSimulator.SimulateAsync(
                               new[] { 1, 2, 3 },
                               evaluator,
                               source.Token))
            {
                observed.Add(result.Result);
            }
        });

        CollectionAssert.AreEqual(new[] { 10 }, observed);
    }

    private readonly record struct NetworkCaseId(int Value);
    private readonly record struct InventoryCaseId(int Value);

    private enum NetworkOutcome
    {
        Success,
        Failure
    }

    private enum InventoryTiming
    {
        Immediate,
        Delayed
    }

    private sealed class CountingEvaluator : IScenarioEvaluator<int, int>
    {
        public int Count { get; private set; }

        public int Evaluate(int scenario)
        {
            Count++;
            return scenario * 2;
        }
    }

    private sealed class CancellingEvaluator(CancellationTokenSource source) : IAsyncScenarioEvaluator<int, int>
    {
        public ValueTask<int> EvaluateAsync(int scenario, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (scenario == 2)
            {
                source.Cancel();
                cancellationToken.ThrowIfCancellationRequested();
            }

            return ValueTask.FromResult(scenario * 10);
        }
    }
}
