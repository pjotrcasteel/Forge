using Forge.Decide;

var cancellationToken = CancellationToken.None;

var fastLane = new FastLaneStrategy();
var balanced = new BalancedStrategy();
var economy = new EconomyStrategy();

var interactive = StrategySpace<InteractiveWorkload>
    .Define<WorkloadContext, WorkloadPlan>("interactive-workload")
    .Add(fastLane)
    .Add(balanced)
    .Build();

var productionPolicy = StrategySelectionPolicies.LowestBy<WorkloadContext, WorkloadPlan, int>(
    static (_, candidate) => candidate.Plan.EstimatedCost);
var shadowPolicy = StrategySelectionPolicies.HighestBy<WorkloadContext, WorkloadPlan, int>(
    static (_, candidate) => candidate.Plan.ResilienceScore);

var context = new WorkloadContext(
    RequiresLowLatency: true,
    CostSensitive: true,
    EstimatedUnits: 80);

Console.WriteLine("=== 1. Evaluate only strategies admitted to the interactive space ===");
var comparison = await interactive.CompareAsync(context, cancellationToken);
PrintComparison(comparison);
Console.WriteLine($"Economy strategy evaluations: {economy.EvaluationCount} (not admitted, so still zero)");

Console.WriteLine();
Console.WriteLine("=== 2. Apply production and shadow policies to the exact same proposals ===");
var shadowDecision = await comparison.DecideWithShadowAsync(context, productionPolicy, shadowPolicy, cancellationToken);
Console.WriteLine($"Production: {shadowDecision.Production.SelectedStrategyId}");
Console.WriteLine($"Shadow:     {shadowDecision.Shadow.SelectedStrategyId}");
Console.WriteLine($"Changed:    {shadowDecision.SelectionChanged}");

Console.WriteLine();
Console.WriteLine("=== 3. Explain the production decision ===");
Console.WriteLine(shadowDecision.Production.Explain());

Console.WriteLine();
Console.WriteLine("=== 4. Create a deterministic decision receipt ===");
var digest = StrategyDecisionDigest.ComputeSha256Hex(
    shadowDecision.Production,
    static plan => $"{plan.Route}|{plan.EstimatedCost}|{plan.ResilienceScore}");
Console.WriteLine(digest);

Console.WriteLine();
Console.WriteLine("=== 5. Re-evaluate after the context changes and diff the evidence ===");
var changedContext = context with { RequiresLowLatency = false };
var changedComparison = await interactive.CompareAsync(changedContext, cancellationToken);
var changedDecision = await changedComparison.DecideAsync(changedContext, productionPolicy, cancellationToken);
var decisionDiff = StrategyDecisionDiff.Between(
    shadowDecision.Production,
    changedDecision,
    static plan => $"{plan.Route}|{plan.EstimatedCost}|{plan.ResilienceScore}");

Console.WriteLine($"Before:             {decisionDiff.BeforeSelectedStrategyId}");
Console.WriteLine($"After:              {decisionDiff.AfterSelectedStrategyId}");
Console.WriteLine($"Selection changed:  {decisionDiff.SelectionChanged}");
Console.WriteLine($"Evidence changed:   {decisionDiff.Evidence.HasChanges}");

Console.WriteLine();
Console.WriteLine("=== 6. A different space may admit a completely different candidate set ===");
var batch = StrategySpace<BatchWorkload>
    .Define<WorkloadContext, WorkloadPlan>("batch-workload")
    .Add(balanced)
    .Add(economy)
    .Build();

var batchComparison = await batch.CompareAsync(
    new WorkloadContext(RequiresLowLatency: false, CostSensitive: true, EstimatedUnits: 500),
    cancellationToken);
PrintComparison(batchComparison);

static void PrintComparison<TSpace>(StrategyComparison<TSpace, WorkloadPlan> comparison)
{
    foreach (var candidate in comparison.Candidates)
    {
        switch (candidate)
        {
            case ApplicableStrategyCandidate<WorkloadPlan> applicable:
                Console.WriteLine(
                    $"[applicable] {applicable.StrategyId,-14} cost={applicable.Plan.EstimatedCost,-3} resilience={applicable.Plan.ResilienceScore,-3} {applicable.Reason}");
                break;
            case RejectedStrategyCandidate<WorkloadPlan> rejected:
                Console.WriteLine($"[rejected]   {rejected.StrategyId,-14} {rejected.Reason}");
                break;
        }
    }
}

internal sealed class InteractiveWorkload
{
}

internal sealed class BatchWorkload
{
}

internal sealed record WorkloadContext(bool RequiresLowLatency, bool CostSensitive, int EstimatedUnits);

internal sealed record WorkloadPlan(string Route, int EstimatedCost, int ResilienceScore);

internal sealed class FastLaneStrategy : IStrategy<WorkloadContext, WorkloadPlan>
{
    public StrategyId Id => "fast-lane";

    public int EvaluationCount { get; private set; }

    public ValueTask<StrategyProposal<WorkloadPlan>> ProposeAsync(WorkloadContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EvaluationCount++;

        if (!context.RequiresLowLatency)
        {
            return ValueTask.FromResult(
                StrategyProposal<WorkloadPlan>.NotApplicable("Low latency is not required."));
        }

        return ValueTask.FromResult(
            StrategyProposal<WorkloadPlan>.Applicable(
                new WorkloadPlan("fast-lane", EstimatedCost: 90, ResilienceScore: 95),
                "Meets the low-latency requirement."));
    }
}

internal sealed class BalancedStrategy : IStrategy<WorkloadContext, WorkloadPlan>
{
    public StrategyId Id => "balanced";

    public ValueTask<StrategyProposal<WorkloadPlan>> ProposeAsync(WorkloadContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return ValueTask.FromResult(
            StrategyProposal<WorkloadPlan>.Applicable(
                new WorkloadPlan("balanced", EstimatedCost: 50, ResilienceScore: 75),
                context.CostSensitive
                    ? "Balances cost and resilience for a cost-sensitive workload."
                    : "Provides the general-purpose route."));
    }
}

internal sealed class EconomyStrategy : IStrategy<WorkloadContext, WorkloadPlan>
{
    public StrategyId Id => "economy";

    public int EvaluationCount { get; private set; }

    public ValueTask<StrategyProposal<WorkloadPlan>> ProposeAsync(WorkloadContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EvaluationCount++;

        if (context.RequiresLowLatency)
        {
            return ValueTask.FromResult(
                StrategyProposal<WorkloadPlan>.NotApplicable("Economy routing does not guarantee low latency."));
        }

        return ValueTask.FromResult(
            StrategyProposal<WorkloadPlan>.Applicable(
                new WorkloadPlan("economy", EstimatedCost: 20, ResilienceScore: 55),
                "The workload can trade latency for lower cost."));
    }
}