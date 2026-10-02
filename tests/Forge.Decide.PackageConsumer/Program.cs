using Forge.Decide;

var space = StrategySpace<InteractiveSpace>
    .Define<RequestContext, ExecutionPlan>("interactive")
    .Add(new FastPathStrategy())
    .Add(new BalancedStrategy())
    .Build();

var decision = await space.DecideAsync(
    new RequestContext(LowLatencyRequired: true),
    CancellationToken.None);

if (decision.SelectedStrategyId.Value != "fast-path" || decision.Plan.Name != "fast-plan")
{
    throw new InvalidOperationException("Forge.Decide package consumer validation failed.");
}

Console.WriteLine("Forge.Decide package consumer validation passed.");

internal sealed class InteractiveSpace
{
}

internal sealed record RequestContext(bool LowLatencyRequired);

internal sealed record ExecutionPlan(string Name);

internal sealed class FastPathStrategy : IStrategy<RequestContext, ExecutionPlan>
{
    public StrategyId Id => "fast-path";

    public ValueTask<StrategyProposal<ExecutionPlan>> ProposeAsync(RequestContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return ValueTask.FromResult(
            context.LowLatencyRequired
                ? StrategyProposal<ExecutionPlan>.Applicable(new ExecutionPlan("fast-plan"), "Low latency is required.")
                : StrategyProposal<ExecutionPlan>.NotApplicable("Low latency is not required."));
    }
}

internal sealed class BalancedStrategy : IStrategy<RequestContext, ExecutionPlan>
{
    public StrategyId Id => "balanced";

    public ValueTask<StrategyProposal<ExecutionPlan>> ProposeAsync(RequestContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(StrategyProposal<ExecutionPlan>.NotApplicable("Fast path is required for this request."));
    }
}