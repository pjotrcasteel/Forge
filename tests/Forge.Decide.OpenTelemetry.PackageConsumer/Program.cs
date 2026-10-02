using Forge.Decide;
using Forge.Decide.OpenTelemetry;

var space = StrategySpace<ConsumerSpace>
    .Define<ConsumerContext, ConsumerPlan>("consumer")
    .Add(new ConsumerStrategy().WithOpenTelemetry())
    .Build();

var decision = await space.DecideWithOpenTelemetryAsync(new ConsumerContext(), CancellationToken.None);

if (decision.SelectedStrategyId.Value != "consumer")
{
    throw new InvalidOperationException("Forge.Decide.OpenTelemetry package consumer validation failed.");
}

Console.WriteLine("Forge.Decide.OpenTelemetry package consumer validation passed.");

internal sealed class ConsumerSpace
{
}

internal sealed record ConsumerContext;

internal sealed record ConsumerPlan;

internal sealed class ConsumerStrategy : IStrategy<ConsumerContext, ConsumerPlan>
{
    public StrategyId Id => "consumer";

    public ValueTask<StrategyProposal<ConsumerPlan>> ProposeAsync(ConsumerContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(StrategyProposal<ConsumerPlan>.Applicable(new ConsumerPlan()));
    }
}